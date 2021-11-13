namespace advisor.Features {
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Newtonsoft.Json;

    public record SetGameOptions(long GameId, GameOptions Options) : IRequest<DbGame> {
    }

    public class SetGameOptionsHandler : IRequestHandler<SetGameOptions, DbGame> {
        public SetGameOptionsHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(SetGameOptions request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) return null;

            game.Options = request.Options;

            await db.SaveChangesAsync();

            return game;
        }
    }
}
