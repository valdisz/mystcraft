namespace advisor;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IGameRepository {
    IQueryable<DbGame> AllGames { get; }

    Task<DbGame> CompleateAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> CreateLocalAsync(string name, long engineId, GameOptions options, Stream playerData, Stream gameData, CancellationToken cancellation = default);
    Task<DbGame> CreateRemoteAsync(string name, string serverAddress, int turnNumber, GameOptions options, CancellationToken cancellation = default);
    Task<DbGame> PauseAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> StartAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> LockAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> UnlockAsync(long gameId, CancellationToken cancellation = default);

    Task<DbGame> GetOneAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> GetOneNoTrackingAsync(long gameId, CancellationToken cancellation = default);
}

public class GameRepository : IGameRepository {
    public GameRepository(IUnitOfWork unit, Database db) {
        this.unit = unit;
        this.db = db;
    }

    private readonly IUnitOfWork unit;
    private readonly Database db;

    public IQueryable<DbGame> AllGames => db.Games;

    public async Task<DbGame> CreateLocalAsync(string name, long engineId, GameOptions options, Stream playerData, Stream gameData, CancellationToken cancellation) {
        await unit.BeginTransactionAsync(cancellation);

        var game = new DbGame {
            Name = name,
            Type = GameType.LOCAL,
            Status = GameStatus.NEW,
            CreatedAt = DateTimeOffset.UtcNow,
            Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
            EngineId = engineId,
            Options = options
        };

        await db.Games.AddAsync(game, cancellation);
        await unit.SaveChangesAsync(cancellation);

        ///// seed turn

        var seedTurn = new DbTurn
        {
            GameId = game.Id,
            Number = 1,
            Status =  TurnStatus.PENDING,
            PlayerData = await playerData.ReadAllBytesAsync(cancellation),
            GameData = await gameData.ReadAllBytesAsync(cancellation)
        };
        await db.Turns.AddAsync(seedTurn, cancellation);

        game.NextTurn = seedTurn;

        /////

        await unit.SaveChangesAsync(cancellation);
        await unit.CommitTransactionAsync(cancellation);

        return game;
    }

    public async Task<DbGame> CreateRemoteAsync(string name, string serverAddress, int turnNumber, GameOptions options, CancellationToken cancellation) {
        options.ServerAddress = serverAddress;

        await unit.BeginTransactionAsync(cancellation);

        var game = new DbGame {
            Name = name,
            Type = GameType.REMOTE,
            Status = GameStatus.NEW,
            CreatedAt = DateTimeOffset.UtcNow,
            Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
            Options = options
        };

        await db.Games.AddAsync(game, cancellation);
        await unit.SaveChangesAsync(cancellation);

        ///// seed turn

        var currentTurn = new DbTurn
        {
            GameId = game.Id,
            Number = turnNumber,
            Status = TurnStatus.READY
        };

        await db.Turns.AddAsync(currentTurn, cancellation);
        game.LastTurn = currentTurn;

        var nextTurn = new DbTurn
        {
            GameId = game.Id,
            Number = turnNumber + 1,
            Status = TurnStatus.PENDING
        };

        await db.Turns.AddAsync(nextTurn, cancellation);
        game.NextTurn = nextTurn;

        /////

        await unit.SaveChangesAsync(cancellation);
        await unit.CommitTransactionAsync(cancellation);

        return game;
    }

    public async Task<DbGame> StartAsync(long gameId, CancellationToken cancellation) {
        var game = await db.Games.FindAsync(gameId);

        if (game?.Status == GameStatus.NEW || game?.Status == GameStatus.PAUSED) {
            game.Status = GameStatus.RUNNING;
        }

        return game;
    }

    public async Task<DbGame> PauseAsync(long gameId, CancellationToken cancellation) {
        var game = await db.Games.FindAsync(gameId);

        if (game?.Status == GameStatus.RUNNING) {
            game.Status = GameStatus.PAUSED;
        }

        return game;
    }

    public async Task<DbGame> CompleateAsync(long gameId, CancellationToken cancellation) {
        var game = await db.Games.FindAsync(gameId);

        if (game != null) {
            game.Status = GameStatus.COMPLEATED;
        }

        return game;
    }

    public async Task<DbGame> LockAsync(long gameId, CancellationToken cancellation) {
        var game = await db.Games.FindAsync(gameId);

        if (game?.Status == GameStatus.RUNNING) {
            game.Status = GameStatus.LOCKED;
        }

        return game;
    }

    public async Task<DbGame> UnlockAsync(long gameId, CancellationToken cancellation) {
        var game = await db.Games.FindAsync(gameId);

        if (game?.Status == GameStatus.LOCKED) {
            game.Status = GameStatus.RUNNING;
        }

        return game;
    }

    public Task<DbGame> GetOneAsync(long gameId, CancellationToken cancellation = default) => AllGames.SingleOrDefaultAsync(x => x.Id == gameId, cancellation);
    public Task<DbGame> GetOneNoTrackingAsync(long gameId, CancellationToken cancellation = default) => AllGames.AsNoTracking().SingleOrDefaultAsync(x => x.Id == gameId, cancellation);
}
