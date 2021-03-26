namespace advisor.Features {
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record SetRuleset(long GameId, string Ruleset) : IRequest<DbGame> {
    }

    public class SetRulesetHandler : IRequestHandler<SetRuleset, DbGame> {
        public SetRulesetHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(SetRuleset request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) return null;

            game.Ruleset = request.Ruleset;

            await db.SaveChangesAsync();

            return game;
        }
    }
}
