namespace advisor.Features {
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record GameRulesetSet(long GameId, string Ruleset) : IRequest<DbGame> {
    }

    public class GameRulesetSetHandler : IRequestHandler<GameRulesetSet, DbGame> {
        public GameRulesetSetHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(GameRulesetSet request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) return null;

            game.Ruleset = request.Ruleset;

            await db.SaveChangesAsync();

            return game;
        }
    }
}
