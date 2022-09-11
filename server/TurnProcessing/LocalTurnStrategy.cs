namespace advisor.TurnProcessing;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public class LocalTurnStrategy : ITurnExecutionStrategy {
    public LocalTurnStrategy(DbGame game, IUnitOfWork unit) {
        this.game = game;
        this.unit = unit;
    }

    private readonly DbGame game;
    private readonly IUnitOfWork unit;

    record GameProjection(long Id, byte[] Engine, int? LastTurnNumber, int? NextTurnNumber);

    record PlayerProjection(long Id, int? Number, string Name, string Password) {
        public bool IsNew => Number == null;
    }

    public async Task RunNextAsync(CancellationToken cancellationToken = default) {
        var game = await db.Games
            .AsNoTracking()
            .Where(x => x.Id == gameId)
            .Include(x => x.Engine)
            .Select(x => new GameProjection(x.Id, x.Engine.Contents, x.LastTurnNumber ?? 0, x.NextTurnNumber ?? 0))
            .FirstOrDefaultAsync(cancellationToken);

        var lastTurn = await db.Turns
            .AsNoTracking()
            .InGame(gameId)
            .Where(x => x.Number == game.LastTurnNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextTurn = await db.Turns
            .InGame(gameId)
            .Where(x => x.Number == game.NextTurnNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var players = await db.Players
            .AsNoTracking()
            .InGame(gameId)
            .OnlyActivePlayers()
            .Select(x => new PlayerProjection(x.Id, x.Number, x.Name, x.Password))
            .OrderBy(x => x.Number)
            .ToListAsync(cancellationToken);

        var options = TurnRunnerOptions.UseTempDirectory();
        var runner = new TurnRunner(options);
        try {
            await BeforeTurnAsync(db, runner, game, lastTurn, players, cancellationToken);

            var run = await runner.RunAsync(TimeSpan.FromMinutes(10), cancellationToken);
            if (run.Success) {
                await AfterTurnAsync(db, runner, game, nextTurn, players, cancellationToken);

                await db.SaveChangesAsync();

                // return new GameTurnRunResult(true, null);
            }

            // return new GameTurnRunResult(false, run.StdErr);
        }
        finally {
            runner.Clean();
            Directory.Delete(options.WorkingDirectory);
        }
    }

    private async Task BeforeTurnAsync(
        Database db,
        TurnRunner runner,
        GameProjection game,
        DbTurn lastTurn,
        IEnumerable<PlayerProjection> players,
        CancellationToken cancellationToken
    ) {
        await runner.WriteEngineAsync(game.Engine);

        await runner.WriteGameAsync(lastTurn.GameData);

        var playersIn = AppendNewFactions(players.Where(x => x.IsNew), lastTurn.PlayerData);
        await runner.WritePlayersAsync(playersIn);

        foreach (var player in players.Where(x => !x.IsNew)) {
            var orders = await db.Units
                .Where(x => x.PlayerId == player.Id && x.FactionNumber == player.Number && x.TurnNumber == lastTurn.Number)
                .Select(x => new UnitOrdersRec(x.Number, x.Orders))
                .ToListAsync(cancellationToken);

            await runner.WriteFactionOrdersAsync(player.Number.Value, player.Password, orders);
        }
    }

    private async Task AfterTurnAsync(
        Database db,
        TurnRunner runner,
        GameProjection game,
        DbTurn nextTurn,
        IEnumerable<PlayerProjection> players,
        CancellationToken cancellationToken
    ) {
        var gameOut = runner.GetGameOut();
        nextTurn.GameData = await gameOut.ReadAllBytesAsync(cancellationToken);

        var playersOut = runner.GetPlayersOut();
        nextTurn.PlayerData = await playersOut.ReadAllBytesAsync(cancellationToken);

        foreach (var faction in MatchNewFactions(playersOut, players)) {
            var player = await db.Players.FirstOrDefaultAsync(x => x.Id == faction.Id);
            player.Number = faction.Number;
        }

        // todo: quit factions will be missing from players.out

        foreach (var article in runner.ListArticles()) {
            using var reader = article.OpenText();

            var text = await reader.ReadToEndAsync();
            nextTurn.Articles.Add(new DbArticle { Text = text, Type = "engine" });
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

            nextTurn.Reports.Add(report);
        }
    }

    private string AppendNewFactions(IEnumerable<PlayerProjection> players, byte[] playerData) {
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

    private List<PlayerProjection> MatchNewFactions(FileInfo playersOut, IEnumerable<PlayerProjection> players) {
        var newFactions = players
            .Where(x => x.Number == null)
            .ToList();

        if (newFactions.Count > 0) {
            using var playersFileStream = playersOut.OpenRead();
            var factions = ReadFactions(playersFileStream)
                .TakeLast(newFactions.Count)
                .ToList();

            for (var i = 0; i < factions.Count; i++) {
                newFactions[i] = newFactions[i] with {
                    Number = factions[i].Number.Value
                };
            }
        }

        return newFactions;
    }

    private IEnumerable<FactionRecord> ReadFactions(Stream playersFileStream) => new PlayersFileReader(playersFileStream);
}
