namespace advisor.Features;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using advisor.TurnProcessing;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

public record GameTurnRun(long GameId): IRequest<GameTurnRunResult>;
public record GameTurnRunResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

[System.Serializable]
public class GameTurnRunException : System.Exception
{
    public GameTurnRunException() { }
    public GameTurnRunException(string message) : base(message) { }
    public GameTurnRunException(string message, System.Exception inner) : base(message, inner) { }
    protected GameTurnRunException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class GameTurnRunHandler : IRequestHandler<GameTurnRun, GameTurnRunResult> {
    public GameTurnRunHandler(IUnitOfWork unit, IHttpClientFactory httpFactory) {
        this.unit = unit;
        this.httpFactory = httpFactory;
    }

    record GameProjection(long Id, byte[] Engine, int? LastTurnNumber, int? NextTurnNumber, Persistence.GameType Type, GameOptions Options);

    record FactionProjection(long Id, int? Number, string Name, string Password) {
        public bool IsNew => Number == null;
        public bool IsClaimed => !string.IsNullOrWhiteSpace(Password);
    }

    private readonly IUnitOfWork unit;
    private readonly IHttpClientFactory httpFactory;

    public async Task<GameTurnRunResult> Handle(GameTurnRun request, CancellationToken cancellationToken) {
        var game = await unit.Games.GetOneAsync(request.GameId, cancellationToken);
        if (game == null) {
            return new GameTurnRunResult(false, "Game not found.");
        }

        switch (game.Status) {
            case GameStatus.NEW:
                return new GameTurnRunResult(false, "Game not yet started.");

            case GameStatus.RUNNING:
                await unit.Games.LockAsync(request.GameId, cancellationToken);
                await unit.SaveChangesAsync(cancellationToken);
                break;

            case GameStatus.LOCKED:
                break;

            case GameStatus.PAUSED:
                return new GameTurnRunResult(false, "Game is paused.");

            case GameStatus.COMPLEATED:
                return new GameTurnRunResult(false, "Game already compleated.");

            default:
                return new GameTurnRunResult(false, "Game is in the unknonw state.");
        }

        if (game.NextTurnNumber == null) {
            return new GameTurnRunResult(false, "Next turn is not defined.");
        }
        var turnNumber = game.NextTurnNumber.Value;

        var playersRepo = unit.Players(game);
        var turnsRepo = unit.Turns(game);

        await unit.BeginTransactionAsync(cancellationToken);

        var factions = await playersRepo.AllActivePlayers
            .AsNoTracking()
            .Select(x => new FactionProjection(x.Id, x.Number, x.Name, x.Password))
            .ToListAsync(cancellationToken);

        DbTurn turn = await turnsRepo.GetOneAsync(turnNumber, cancellationToken);
        DbTurn nextTurn;

        switch (game.Type) {
            case Persistence.GameType.LOCAL:
                // run local turn
                var engine = await unit.Engines.GetOneNoTrackingAsync(game.EngineId.Value, cancellationToken);

                var allOrders = new Dictionary<FactionProjection, IEnumerable<UnitOrdersRec>>();
                foreach (var player in factions.Where(x => !x.IsNew)) {
                    var playerRepo = await unit.PlayerAsync(player.Id);
                    var orders = await playerRepo.Orders(turnNumber)
                        .Select(x => new UnitOrdersRec(x.UnitNumber, x.Orders))
                        .ToListAsync(cancellationToken);

                    allOrders.Add(player, orders);
                }

                var newFactions = factions.Where(x => x.IsNew).ToList();
                var playersIn = AppendNewFactions(newFactions, turn.PlayerData);

                try {
                    nextTurn = await RunLocalTurnAsync(turnsRepo, playersRepo, game, turn, allOrders, newFactions, engine.Contents, turn.GameData, playersIn, cancellationToken);
                }
                catch (GameTurnRunException ex) {
                    await unit.RollbackTransactionAsync(cancellationToken);
                    return new GameTurnRunResult(false, ex.Message);
                }

                break;

            case Persistence.GameType.REMOTE:
                // import remote turn

                var client = new NewOriginsClient(game.Options.ServerAddress, httpFactory);
                var remoteTurnNumber = await client.GetCurrentTurnNumberAsync(cancellationToken);

                if (remoteTurnNumber > turnNumber) {
                    // some turns were missing
                    await turnsRepo.UpdateTurnNumberAsync(turnNumber, remoteTurnNumber, cancellationToken);
                    turnNumber = remoteTurnNumber;
                }

                if (remoteTurnNumber < turnNumber) {
                    // inconsisntent data or no turn yet
                }

                nextTurn = await ImportRemoteTurnAsync(turnsRepo, playersRepo, client, game, turn, factions, cancellationToken);

                break;

            default:
                await unit.RollbackTransactionAsync(cancellationToken);
                return new GameTurnRunResult(false, "Game is of unknown type, just LOCAL or REMOTE games are supported.");
        }

        turn.Status = TurnStatus.EXECUTED;

        await unit.SaveChangesAsync(cancellationToken);
        await unit.CommitTransactionAsync(cancellationToken);

        return new GameTurnRunResult(true, Game: game);
    }

    private async Task<DbTurn> RunLocalTurnAsync(
        ITurnsRepository turnsRepo,
        IPlayersRepository playersRepo,
        DbGame game,
        DbTurn turn,
        Dictionary<FactionProjection, IEnumerable<UnitOrdersRec>> allOrders,
        List<FactionProjection> newFactions,
        byte[] gameEngine,
        byte[] gameData,
        string playersIn,
        CancellationToken cancellation
    ) {
        List<FactionRecord> factionsOut;
        DbTurn nextTurn;

        // 1. Start the local turn runner in random temp directory
        var runner = new TurnRunner(TurnRunnerOptions.UseTempDirectory());
        try {
            // 2. Put there engine and game state files
            await runner.WriteEngineAsync(gameEngine);
            await runner.WriteGameAsync(gameData);
            await runner.WritePlayersAsync(playersIn);

            // 3. Write faction orders
            foreach (var (faction, orders) in allOrders) {
                await runner.WriteFactionOrdersAsync(faction.Number.Value, faction.Password, orders);
            }

            // 4. Run the simulation
            var run = await runner.RunAsync(TimeSpan.FromMinutes(10), cancellation);
            if (!run.Success) {
                throw new GameTurnRunException(run.StdErr);
            }

            /////

            // 5. Read new game state
            var gameOut = runner.GetGameOut();
            var playersOut = runner.GetPlayersOut();

            // 6. Store game state into the upcoming turn
            nextTurn = await turnsRepo.GetOrCreateNextTurnAsync(turn.Number + 1, cancellation);
            nextTurn.GameData = await gameOut.ReadAllBytesAsync(cancellation);
            nextTurn.PlayerData = await playersOut.ReadAllBytesAsync(cancellation);

            // 7. Read players
            using var playersOutStream = playersOut.OpenRead();
            factionsOut = ReadFactions(playersOutStream).ToList();
        }
        finally {
            runner.Clean();
        }

        foreach (var faction in MatchWithNewFactions(factionsOut, newFactions)) {
            var player = await playersRepo.GetOneAsync(faction.Id);
            player.Number = faction.Number.Value;
        }

        foreach (var faction in MatchWithQuitFactions(factionsOut, allOrders.Keys)) {
            await playersRepo.QuitAsync(faction.Id, cancellation);
        }

        foreach (var article in runner.ListArticles()) {
            using var reader = article.OpenText();

            var text = await reader.ReadToEndAsync();
            turn.Articles.Add(new DbArticle { Text = text, Type = "Global" });
        }

        var errors = false;
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
                Data = ms.ToArray(),
            };

            try {
                ms.Seek(0, SeekOrigin.Begin);
                var json = await ParseReportAsync(ms, cancellation);
                var jsonString = json.ToString(Newtonsoft.Json.Formatting.None);
                report.Json = Encoding.UTF8.GetBytes(jsonString);
            }
            catch (Exception ex) {
                report.Error = ex.ToString();
                errors = true;
            }

            turn.Reports.Add(report);
        }

        if (errors) {
            turn.Status = TurnStatus.ERROR;
        }

        return nextTurn;
    }

    private async Task<DbTurn> ImportRemoteTurnAsync(
        ITurnsRepository turnsRepo,
        IPlayersRepository playersRepo,
        NewOriginsClient client,
        DbGame game,
        DbTurn turn,
        List<FactionProjection> factions,
        CancellationToken cancellation
    ) {
        bool errors = false;
        foreach (var faction in factions.Where(x => x.IsClaimed)) {
            var report = new DbReport {
                FactionNumber = faction.Number.Value,
            };

            string reportText;
            try {
                reportText = await client.DownloadReportAsync(faction.Number.Value, faction.Password, cancellation);
                report.Data = Encoding.UTF8.GetBytes(reportText);

                using var reader = new StringReader(reportText);
                var json = await ParseReportAsync(reader, cancellation);
                var jsonString = json.ToString(Newtonsoft.Json.Formatting.None);
                report.Json = Encoding.UTF8.GetBytes(jsonString);
            }
            catch (Exception ex) {
                report.Error = ex.ToString();
                errors = true;
            }

            turn.Reports.Add(report);
        }

        if (errors) {
            turn.Status = TurnStatus.ERROR;
        }

        await foreach (var faction in client.ListFactionsAsync(cancellation)) {
            if (!faction.Number.HasValue) {
                continue;
            }

            var player = await playersRepo.GetOneByNumberAsync(faction.Number.Value, cancellation);
            if (player == null) {
                await playersRepo.AddRemoteAsync(faction.Number.Value, faction.Name, cancellation);
            }
            else {
                player.Name = faction.Name;
            }
        }

        var nextTurn = await turnsRepo.GetOrCreateNextTurnAsync(turn.Number + 1, cancellation);
        return nextTurn;
    }

    private async Task<JObject> ParseReportAsync(Stream source, CancellationToken cancellation) {
        using var reader = new StreamReader(source);
        return await ParseReportAsync(reader, cancellation);
    }

    private async Task<JObject> ParseReportAsync(TextReader reader, CancellationToken cancellation) {
        using var atlantisReader = new AtlantisReportJsonConverter(reader);
        var json = await atlantisReader.ReadAsJsonAsync();

        return json;
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

    private List<FactionProjection> MatchWithNewFactions(List<FactionRecord> nextFactions, List<FactionProjection> newFactions) {
        var i = 0;
        foreach (var faction in nextFactions.TakeLast(newFactions.Count)) {
            newFactions[i] = newFactions[i] with {
                Number = faction.Number.Value
            };

            i++;
        }

        return newFactions;
    }

    private List<FactionProjection> MatchWithQuitFactions(List<FactionRecord> nextFactions, IEnumerable<FactionProjection> factions) {
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
