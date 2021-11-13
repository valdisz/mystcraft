namespace advisor.Features {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record RunLocalTurn(long GameId): IRequest<DbGameTurn>;

    public record PlayerRec(int? Number) {
        public Dictionary<string, string> Props { get; init; } = new ();
    }

    public record PlayerProjection(long Id, int? Number, string Name, string Password);

    public class RunLocalTurnHandler : IRequestHandler<RunLocalTurn, DbGameTurn> {
        public RunLocalTurnHandler(Database db, IMediator mediator) {
            this.db = db;
            this.mediator = mediator;
        }

        private readonly Database db;
        private readonly IMediator mediator;

        public async Task<DbGameTurn> Handle(RunLocalTurn request, CancellationToken cancellationToken) {
            var game = await db.Games
                .Where(x => x.Id == request.GameId)
                .Select(x => new { x.Id, x.Engine })
                .FirstOrDefaultAsync();

            var lastTurn = await db.GameTurns
                .OrderByDescending(x => x.Number)
                .FirstOrDefaultAsync();

            var players = (await db.Players
                .Where(x => x.GameId == request.GameId && !x.IsQuit)
                .OrderBy(x => x.Number)
                .Select(x => new { x.Id, x.Name, x.Number, x.Password })
                .ToListAsync())
                .Select(x => new PlayerProjection(x.Id, x.Number, x.Name, x.Password))
                .ToList();

            var playersIn = ComposePlayersIn(players, lastTurn.PlayerData);

            var executor = new TurnExecutor();
            executor.CreateWorkDirectory();

            await executor.WritePlayersInAsync(playersIn);
            await executor.WriteGameInAsync(lastTurn.GameData);

            foreach (var player in players) {
                if (player.Number == null) {
                    continue;
                }

                var orders = (await db.Units
                    .Where(x => x.PlayerId == player.Id && x.FactionNumber == player.Number && x.TurnNumber == lastTurn.Number)
                    .Select(x => new { x.Number, x.Orders })
                    .ToListAsync())
                    .Select(x => new UnitOrdersRec(x.Number, x.Orders));

                await executor.WriteFactionOrdersAsync(player.Number.Value, player.Password, orders);
            }

            DbGameTurn newTurn = null;
            if (await executor.RunAsync(game.Engine, TimeSpan.FromMinutes(10))) {
                newTurn = await ReadNewTurn(request.GameId, lastTurn.Number + 1, executor);

                var newFactions = AcceptNewFactions(players, executor);
                if (newFactions.Count > 0) {
                    foreach (var nf in newFactions) {
                        var player = await db.Players.FirstOrDefaultAsync(x => x.Id == nf.playerId);
                        player.Number = nf.factionNumber;
                    }

                    await db.SaveChangesAsync();
                }

                foreach (var report in executor.ReadReports()) {
                    var player = players.Find(x => x.Number == report.Number);
                    if (player == null) {
                        report.ReportStream.Dispose();
                        continue;
                    }

                    int earliestTurn;
                    using (report.ReportStream) {
                        using var reader = new StreamReader(report.ReportStream);
                        var reportText = await reader.ReadToEndAsync();

                        earliestTurn = await mediator.Send(new UploadReports(player.Id, new [] { reportText }));
                    }

                    await mediator.Send(new ProcessTurn(player.Id, earliestTurn));
                }
            }

            return newTurn;
        }

        private List<(long playerId, int factionNumber)> AcceptNewFactions(List<PlayerProjection> players, TurnExecutor executor) {
            using var playersOut = executor.OpenPlayersOut();
            using var reader = new StreamReader(playersOut);

            var newFactions = new List<(long playerId, int factionNumber)>();

            var list = ReadPlayers(reader).ToList();
            foreach (var np in players.Where(x => x.Number == null)) {
                var rec = list.Find(x => x.Props["Name"].StartsWith($"{np.Name} ("));
                if (rec?.Number == null) {
                    continue;
                }

                newFactions.Add((np.Id, rec.Number.Value));
            }

            return newFactions;
        }

        private string ComposePlayersIn(List<PlayerProjection> players, byte[] playerData) {
            var sb = new StringBuilder();

            using var ms = new MemoryStream(playerData);
            using var reader = new StreamReader(ms);

            sb.Append(reader.ReadToEnd());

            foreach (var np in players.Where(x => x.Number == null)) {
                var password = string.IsNullOrWhiteSpace(np.Password) ? "none" : np.Password;

                sb.AppendLine("Faction: new");
                sb.AppendLine($"Name: {np.Name}");
                sb.AppendLine($"Password: {password}");
                sb.AppendLine("SendTimes: 1");
                sb.AppendLine("Template: long");
            }

            return sb.ToString();
        }

        public async Task<DbGameTurn> ReadNewTurn(long gameId, int turnNumber, TurnExecutor executor) {
            var playersOut = await executor.ReadPlayersOutAsync();
            var gameOut = await executor.ReadGameOutAsync();

            var newTurn = new DbGameTurn {
                GameId = gameId,
                Number = turnNumber,
                PlayerData = playersOut,
                GameData = gameOut
            };

            foreach (var article in executor.ReadTimesAsync()) {
                using var reader = new StreamReader(article);
                newTurn.Articles.Add(new DbGameArticle {
                    Type = "times",
                    Text = await reader.ReadToEndAsync()
                });
            }

            await db.GameTurns.AddAsync(newTurn);
            await db.SaveChangesAsync();

            foreach (var report in executor.ReadReports()) {
            }

            return newTurn;
        }

        public IEnumerable<PlayerRec> ReadPlayers(TextReader reader) {
            PlayerRec rec = null;

            (string key, string value) parse(string s) {
                var kv = s.Split(":");
                return (kv[0].Trim(), kv[1].Trim());
            }

            string line;
            while ((line = reader.ReadLine()) != null) {
                if (line.StartsWith("Faction:")) {
                    if (rec != null) {
                        yield return rec;
                    }

                    var (_, num) = parse(line);
                    rec = new PlayerRec(int.Parse(num));

                    continue;
                }

                if (rec == null) {
                    continue;
                }

                var (key, value) = parse(line);
                rec.Props.Add(key, value);
            }

            if (rec != null) {
                yield return rec;
            }
        }
    }
}
