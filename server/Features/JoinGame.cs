namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record JoinGame(long GameId, long UserId) : IRequest<DbUserGame> {
    }

    public class JoinGameHandler : IRequestHandler<JoinGame, DbUserGame> {
        public JoinGameHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbUserGame> Handle(JoinGame request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.UserId);
            if (game == null) return null;

            var userGame = db.UserGames.SingleOrDefault(x => x.GameId == request.GameId && x.UserId == request.UserId);
            if (userGame == null) {
                userGame = new DbUserGame {
                    GameId = request.GameId,
                    UserId = request.UserId
                };

                await db.UserGames.AddAsync(userGame);
                await db.SaveChangesAsync();
            }

            return userGame;
        }
    }
}
