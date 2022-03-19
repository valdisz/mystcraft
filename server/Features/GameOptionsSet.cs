namespace advisor.Features {
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record GameOptionsSet(long GameId, GameOptions Options) : IRequest<DbGame> {
    }

    public class GameOptionsSetHandler : IRequestHandler<GameOptionsSet, DbGame> {
        public GameOptionsSetHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(GameOptionsSet request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) return null;

            game.Options = request.Options;

            await db.SaveChangesAsync();

            return game;
        }
    }
}
