namespace advisor.Features
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record GameCreateLocal(string Name, long EngineId, GameOptions Options, Stream PlayerData, Stream GameData) : IRequest<GameCreateLocalResult>;

    public record GameCreateLocalResult(DbGame Game, bool IsSuccess, string Error) : IMutationResult;

    public class GameCreateLocalHandler : IRequestHandler<GameCreateLocal, GameCreateLocalResult> {
        public GameCreateLocalHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<GameCreateLocalResult> Handle(GameCreateLocal request, CancellationToken cancellationToken) {
            var newGame = new DbGame {
                Name = request.Name,
                Type = GameType.Local,
                Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
                EngineId = request.EngineId,
                Options = request.Options
            };

            using var playerData = new MemoryStream();
            await request.PlayerData.CopyToAsync(playerData);
            playerData.Seek(0, SeekOrigin.Begin);

            using var gameData = new MemoryStream();
            await request.GameData.CopyToAsync(gameData);
            gameData.Seek(0, SeekOrigin.Begin);

            var lastTurn = new DbGameTurn {
                Number = 0,
                PlayerData = playerData.ToArray(),
                GameData = gameData.ToArray()
            };

            var nextTurn = new DbGameTurn {
                Number = 1
            };

            newGame.Turns.Add(lastTurn);
            newGame.Turns.Add(nextTurn);

            newGame.LastTurn = lastTurn;
            newGame.NextTurn = nextTurn;

            await db.Games.AddAsync(newGame);
            await db.SaveChangesAsync();

            return new GameCreateLocalResult(newGame, true, null);
        }
    }
}
