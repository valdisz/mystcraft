namespace advisor.Features
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record GameCreateLocal(string Name, Stream Engine, GameOptions Options, Stream PlayerData, Stream GameData) : IRequest<DbGame>;

    public class GameCreateLocalHandler : IRequestHandler<GameCreateLocal, DbGame> {
        public GameCreateLocalHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(GameCreateLocal request, CancellationToken cancellationToken) {
            var newGame = new DbGame {
                Name = request.Name,
                Type = GameType.Local,
                Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
                Options = request.Options
            };

            using var engine = new MemoryStream();
            await request.Engine.CopyToAsync(engine);
            engine.Seek(0, SeekOrigin.Begin);

            using var playerData = new MemoryStream();
            await request.PlayerData.CopyToAsync(playerData);
            playerData.Seek(0, SeekOrigin.Begin);

            using var gameData = new MemoryStream();
            await request.GameData.CopyToAsync(gameData);
            gameData.Seek(0, SeekOrigin.Begin);

            using var reader = new StreamReader(gameData);
            var info = ReadGameInfo(reader);

            newGame.Engine = engine.ToArray();
            newGame.EngineVersion = info.EngineVersion;
            newGame.RulesetName = info.RulesetName;
            newGame.RulesetVersion = info.RulesetVersion;

            var zeroTurn = new DbGameTurn {
                Number = 0,
                PlayerData = playerData.ToArray(),
                GameData = gameData.ToArray()
            };

            newGame.Turns.Add(zeroTurn);

            await db.Games.AddAsync(newGame);
            await db.SaveChangesAsync();

            return newGame;
        }

        public GameInfo ReadGameInfo(TextReader reader) {
            reader.ReadLine();
            var engineVersion = int.Parse(reader.ReadLine());
            var rulesetName = reader.ReadLine();
            var rulesetVersion = int.Parse(reader.ReadLine());

            return new GameInfo(rulesetName, FormatVersion(rulesetVersion), FormatVersion(engineVersion));
        }

        public string FormatVersion(int version) {
            var x = version >> 0x10;
            var y = (version & 0xFF00) >> 0x8;
            var z = version & 0xFF;
            return $"{x}.{y}.{z}";
        }
    }

    public record GameInfo(string RulesetName, string RulesetVersion, string EngineVersion);
}
