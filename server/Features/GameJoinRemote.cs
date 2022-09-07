namespace advisor.Features {
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Model;
    using advisor.Persistence;
    using advisor.Remote;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record GameJoinRemote(long UserId, long GameId, int Number, string Password) : IRequest<GameJoinRemoteResult>;

    public record GameJoinRemoteResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

    public class GameJoinRemoteHandler : IRequestHandler<GameJoinRemote, GameJoinRemoteResult> {
        public GameJoinRemoteHandler(IMediator mediator, Database db, NewOriginsClient client) {
            this.mediator = mediator;
            this.db = db;
            this.client = client;
        }

        private readonly IMediator mediator;
        private readonly Database db;
        private readonly NewOriginsClient client;

        public async Task<GameJoinRemoteResult> Handle(GameJoinRemote request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) {
                return new GameJoinRemoteResult(false, "Game does not exist.");
            }

            if (game.LastTurnNumber == null) {
                return new GameJoinRemoteResult(false, "Game does not have any turn.");
            }

            var turnNumber = game.LastTurnNumber;

            var player = db.Players
                .AsNoTracking()
                .InGame(game)
                .OnlyActivePlayers()
                .SingleOrDefault(x => x.UserId == request.UserId);

            if (player != null) {
                return new GameJoinRemoteResult(false, "There already is an active player in this game.");
            }

            string report;
            try {
                report = await client.DownloadReportAsync(request.Number, request.Password);
            }
            catch (WrongFactionOrPasswordException) {
                return new GameJoinRemoteResult(false, "Wrong faction number or password.");
            }
            catch (Exception) {
                return new GameJoinRemoteResult(false, "Cannot reach the server.");
            }

            player = await ReadPlayerFromReportAsync(report);
            player.GameId = request.GameId;
            player.UserId = request.UserId;
            player.Password = request.Password;

            await db.Players.AddAsync(player);
            await db.SaveChangesAsync();

            db.GameReports.Add(new DbGameReport {
                GameId = request.GameId,
                FactionNumber = request.Number,
                TurnNumber = game.LastTurnNumber.Value,
                Data = Encoding.UTF8.GetBytes(report)
            });

            return new GameJoinRemoteResult(true, Player: player);
        }

        private async Task<DbPlayer> ReadPlayerFromReportAsync(string source) {
            using var textReader = new StringReader(source);

            using var atlantisReader = new AtlantisReportJsonConverter(textReader,
                new ReportFactionSection()
            );
            var json = await atlantisReader.ReadAsJsonAsync();
            var report  = json.ToObject<JReport>();

            var faction = report.Faction;

            return new DbPlayer {
                Number = faction.Number,
                Name = faction.Name
            };
        }
    }
}
