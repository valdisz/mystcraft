namespace advisor.Features;

using System;
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
        DbGame game;
        await using (var tx = await db.Database.BeginTransactionAsync()) {
            game = new DbGame {
                Name = request.Name,
                Type = GameType.Local,
                CreatedAt = DateTimeOffset.UtcNow,
                Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
                EngineId = request.EngineId,
                Options = request.Options
            };

            await db.Games.AddAsync(game);

            await db.SaveChangesAsync();

            /////

            using var playerData = new MemoryStream();
            await request.PlayerData.CopyToAsync(playerData);
            playerData.Seek(0, SeekOrigin.Begin);

            using var gameData = new MemoryStream();
            await request.GameData.CopyToAsync(gameData);
            gameData.Seek(0, SeekOrigin.Begin);

            var lastTurn = new DbGameTurn {
                GameId = game.Id,
                Number = 0,
                PlayerData = playerData.ToArray(),
                GameData = gameData.ToArray()
            };

            var nextTurn = new DbGameTurn {
                GameId = game.Id,
                Number = 1
            };

            game.Turns.Add(lastTurn);
            game.Turns.Add(nextTurn);

            game.LastTurn = lastTurn;
            game.NextTurn = nextTurn;

            await db.SaveChangesAsync();

            /////

            await tx.CommitAsync();
        }

        return new GameCreateLocalResult(game, true, null);
    }
}
