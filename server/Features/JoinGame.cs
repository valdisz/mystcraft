namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record JoinGame(long UserId, long GameId) : IRequest<DbPlayer> {
    }

    public class JoinGameHandler : IRequestHandler<JoinGame, DbPlayer> {
        public JoinGameHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbPlayer> Handle(JoinGame request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) return null;

            var player = db.Players.SingleOrDefault(x => x.GameId == request.GameId && x.UserId == request.UserId);
            if (player == null) {
                player = new DbPlayer {
                    GameId = request.GameId,
                    UserId = request.UserId
                };

                await db.Players.AddAsync(player);
                await db.SaveChangesAsync();
            }

            return player;
        }
    }
}
