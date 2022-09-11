namespace advisor.Features;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using advisor.TurnProcessing;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GameTurnRun(long GameId): IRequest<GameTurnRunResult>;
public record GameTurnRunResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

public class GameTurnRunHandler : IRequestHandler<GameTurnRun, GameTurnRunResult> {
    public GameTurnRunHandler(IUnitOfWork unit, IHttpClientFactory httpClientFactory) {
        this.unit = unit;
        this.httpClientFactory = httpClientFactory;
    }

    record GameProjection(long Id, byte[] Engine, int? LastTurnNumber, int? NextTurnNumber, Persistence.GameType Type, GameOptions Options);

    record FactionProjection(long Id, int? Number, string Name, string Password) {
        public bool IsNew => Number == null;
    }

    private readonly IUnitOfWork unit;
    private readonly IHttpClientFactory httpClientFactory;

    public async Task<GameTurnRunResult> Handle(GameTurnRun request, CancellationToken cancellationToken) {
        var game = await unit.Games.GetOneAsync(request.GameId, cancellationToken);
        if (game == null) {
            return new GameTurnRunResult(false, "Game not found.");
        }

        if (game.Status == GameStatus.COMPLEATED) {
            return new GameTurnRunResult(false, "Game already compleated.");
        }

        if (game.Status == GameStatus.PAUSED) {
            return new GameTurnRunResult(false, "Game is paused.");
        }

        if (game.Status == GameStatus.NEW) {
            return new GameTurnRunResult(false, "Game not yet started.");
        }

        var players = unit.Players(game);
        var turns = unit.Turns(game);

        var factions = await players.AllActivePlayers
            .AsNoTracking()
            .Select(x => new FactionProjection(x.Id, x.Number, x.Name, x.Password))
            .ToListAsync(cancellationToken);


        if (game.Type == Persistence.GameType.LOCAL) {
            // run local turn
            var engine = await unit.Engines.GetOneNoTrackingAsync(game.EngineId.Value, cancellationToken);
            var orders = new Dictionary<long, IEnumerable<UnitOrdersRec>>();

            // when running initial turn, NextTurnNumber will be NULL then we will take data from last turn
            DbTurn turn;
            byte[] gameIn;
            byte[] playersIn;

            if (game.NextTurnNumber == null) {
                DbTurn inputTurn = await turns.GetOneNoTrackingAsync(game.LastTurnNumber.Value, cancellationToken);
                gameIn = inputTurn.GameData;
                playersIn = inputTurn.PlayerData;

                turn = new DbTurn {

                };
            }
            else {
                turn = await turns.GetOneAsync(game.NextTurnNumber.Value, cancellationToken);
                gameIn = turn.GameData;
                playersIn = turn.PlayerData;
            }

            // the next turn
            DbTurn nextTurn = new DbTurn {

            };

            var unitsQuery = await players.GetOwnUnitsAsync(player.Id, turnNumber, player.Number, cancellation);
            var orders = await unitsQuery
                .AsNoTracking()
                .Select(x => new UnitOrdersRec(x.Number, x.Orders))
                .ToListAsync(cancellation);

            return await RunLocalTurnAsync(game, players, factions, orders, turn, nextTurn, engine.Contents, gameIn, playersIn, cancellationToken);
        }

        if (game.Type == Persistence.GameType.REMOTE) {
            // import remote turn

            var client = new NewOriginsClient(game.Options.ServerAddress, httpClientFactory);
            var remoteTurnNumber = await client.GetCurrentTurnNumberAsync(cancellationToken);

            if (remoteTurnNumber != turnNumber) {
                return new GameTurnRunResult(false, "Remote server does not have a new turn yet.");
            }

            return await ImportRemoteTurnAsync(client, game, players, factions, turnNumber, turn, nextTurn, cancellationToken);
        }
    }

    private Task<GameTurnRunResult> ImportRemoteTurnAsync(NewOriginsClient client, DbGame game, IPlayersRepository players, List<FactionProjection> factions, int turnNumber, DbTurn turn, DbTurn nextTurn, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    private async Task<GameTurnRunResult> RunLocalTurnAsync(
        DbGame game,
        IPlayersRepository players,
        List<FactionProjection> factions,
        Dictionary<long, IEnumerable<UnitOrdersRec>> orders,
        DbTurn turn,
        DbTurn nextTurn,
        byte[] gameEngine, byte[] gameData, byte[] playersData, CancellationToken cancellation
    ) {
        var runner = new TurnRunner(TurnRunnerOptions.UseTempDirectory());
        try {
            await runner.WriteEngineAsync(gameEngine);
            await runner.WriteGameAsync(gameData);

            var playersIn = AppendNewFactions(factions.Where(x => x.IsNew), playersData);
            await runner.WritePlayersAsync(playersIn);

            foreach (var (number, factionOrders) in orders) {
                await runner.WriteFactionOrdersAsync(player.Number.Value, player.Password, factionOrders);
            }

            var run = await runner.RunAsync(TimeSpan.FromMinutes(10), cancellation);
            if (!run.Success) {
                return new GameTurnRunResult(false, run.StdErr);
            }

            var gameOut = runner.GetGameOut();
            nextTurn.GameData = await gameOut.ReadAllBytesAsync(cancellation);

            var playersOut = runner.GetPlayersOut();
            using var playersOutStream = playersOut.OpenRead();

            nextTurn.PlayerData = await playersOut.ReadAllBytesAsync(cancellation);

            var nextFactions = ReadFactions(playersOutStream).ToList();

            foreach (var faction in MatchNewFactions(nextFactions, factions.Where(x => x.IsNew).ToList())) {
                var player = await players.GetOneAsync(faction.Id, cancellation);
                player.Number = faction.Number;
            }

            foreach (var faction in MatchQuitFactions(nextFactions, factions.Where(x => !x.IsNew))) {
                await players.QuitAsync(faction.Id, cancellation);
            }

            foreach (var article in runner.ListArticles()) {
                using var reader = article.OpenText();

                var text = await reader.ReadToEndAsync();
                turn.Articles.Add(new DbArticle { Text = text, Type = "engine" });
            }

            var tempaltes = runner.ListTemplates().ToDictionary(x => x.Number);
            foreach (var reportFile in runner.ListReports()) {
                using var ms = new MemoryStream();

                using var reportStream = reportFile.Contents.OpenRead();
                await reportStream.CopyToAsync(ms);

                if (tempaltes.ContainsKey(reportFile.Number)) {
                    using var templateStream = tempaltes[reportFile.Number].Contents.OpenRead();
                    await templateStream.CopyToAsync(ms);
                }

                var report = new DbReport {
                    FactionNumber = reportFile.Number,
                    Data = ms.ToArray()
                };

                turn.Reports.Add(report);
            }

            await unit.SaveChangesAsync(cancellation);

            return new GameTurnRunResult(true, Game: game);
        }
        finally {
            runner.Clean();
        }
    }

    private string AppendNewFactions(IEnumerable<FactionProjection> players, byte[] playerData) {
        using var ms = new MemoryStream(playerData);
        using var reader = new StreamReader(ms);

        using var writer = new StringWriter();
        writer.Write(reader.ReadToEnd());

        var playersWriter = new PlayersFileWriter(writer);

        foreach (var player in players) {
            var faction = new FactionRecord {
                Name = player.Name,
                Password = player.Password,
                SendTimes = true,
                Template = "long"
            };

            playersWriter.Write(faction);
        }

        writer.Flush();

        return writer.ToString();
    }

    private List<FactionProjection> MatchNewFactions(List<FactionRecord> nextFactions, List<FactionProjection> newFactions) {
        var i = 0;
        foreach (var faction in nextFactions.TakeLast(newFactions.Count)) {
            newFactions[i] = newFactions[i] with {
                Number = faction.Number.Value
            };

            i++;
        }

        return newFactions;
    }

    private List<FactionProjection> MatchQuitFactions(List<FactionRecord> nextFactions, IEnumerable<FactionProjection> factions) {
        var knownFactions = nextFactions
            .Select(x => x.Number.Value)
            .ToHashSet();

        var quitFactions = factions
            .Where(x => !knownFactions.Contains(x.Number.Value))
            .ToList();

        return quitFactions;
    }

    private IEnumerable<FactionRecord> ReadFactions(Stream playersFileStream) => new PlayersFileReader(playersFileStream);
}
