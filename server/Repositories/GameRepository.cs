namespace advisor;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IGameRepository {
    IQueryable<DbGame> Games { get; }

    Task<DbGame> GetOneAsync(long gameId, bool withTracking = true, CancellationToken cancellation = default);

    Task<DbGame> CreateLocalAsync(string name, long engineId, string ruleset, GameOptions options, CancellationToken cancellation = default);
    Task<DbGame> CreateRemoteAsync(string name, string serverAddress, string ruleset, GameOptions options, CancellationToken cancellation = default);
}

public class GameRepository : IGameRepository {
    public GameRepository(ITime time, IUnitOfWork unit) {
        this.time = time;
        this.unit = unit;
        this.db = unit.Database;
    }

    private readonly ITime time;
    private readonly IUnitOfWork unit;
    private readonly Database db;

    public IQueryable<DbGame> Games => db.Games;

    public Task<DbGame> GetOneAsync(long gameId, bool withTracking = true, CancellationToken cancellation = default) {
        return db.Games.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Id == gameId, cancellation);
    }

    public async Task<DbGame> CreateLocalAsync(string name, long engineId, string ruleset, GameOptions options, CancellationToken cancellation = default) {
        var game = new DbGame {
            Name = name,
            Type = GameType.LOCAL,
            Status = GameStatus.NEW,
            CreatedAt = time.UtcNow,
            EngineId = engineId,
            Ruleset = ruleset,
            Options = options,
            NextTurnNumber = 1
        };

        await db.Games.AddAsync(game, cancellation);

        return game;
    }

    public async Task<DbGame> CreateRemoteAsync(string name, string serverAddress, string ruleset, GameOptions options, CancellationToken cancellation = default) {
        var game = new DbGame {
            Name = name,
            Type = GameType.REMOTE,
            Status = GameStatus.NEW,
            Ruleset = ruleset,
            CreatedAt = time.UtcNow,
            Options = options with {
                ServerAddress = serverAddress
            }
        };

        await db.Games.AddAsync(game, cancellation);

        return game;
    }
}
