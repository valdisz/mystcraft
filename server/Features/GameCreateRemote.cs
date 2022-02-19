namespace advisor.Features {
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record GameCreateRemote(string Name, string EngineVersion, string RulesetName, string RulesetVersion, GameOptions Options) : IRequest<DbGame>;

    public class GameCreateRemoteHandler : IRequestHandler<GameCreateRemote, DbGame> {
        public GameCreateRemoteHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(GameCreateRemote request, CancellationToken cancellationToken) {
            var newGame = new DbGame {
                Name = request.Name,
                Type = GameType.Remote,
                Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
                Options = request.Options,
                EngineVersion = request.EngineVersion,
                RulesetName = request.RulesetName,
                RulesetVersion = request.RulesetVersion
            };

            await db.Games.AddAsync(newGame);
            await db.SaveChangesAsync();

            return newGame;
        }
    }
}
