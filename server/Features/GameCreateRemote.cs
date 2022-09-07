namespace advisor.Features {
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record GameCreateRemote(string Name, GameOptions Options) : IRequest<GameCreateRemoteResult>;

    public record GameCreateRemoteResult(DbGame Game, bool IsSuccess, string Error) : IMutationResult;

    public class GameCreateRemoteHandler : IRequestHandler<GameCreateRemote, GameCreateRemoteResult> {
        public GameCreateRemoteHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<GameCreateRemoteResult> Handle(GameCreateRemote request, CancellationToken cancellationToken) {
            var newGame = new DbGame {
                Name = request.Name,
                Type = GameType.REMOTE,
                Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
                Options = request.Options
            };

            await db.Games.AddAsync(newGame);
            await db.SaveChangesAsync();

            return new GameCreateRemoteResult(newGame, true, null);
        }
    }
}
