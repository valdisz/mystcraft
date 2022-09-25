namespace advisor;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IGameRepository {
    IQueryable<DbGame> Games { get; }

    Task<DbGame> GetOneAsync(long gameId);
    Task<DbGame> GetOneNoTrackingAsync(long gameId, CancellationToken cancellation = default);

    Task<DbGame> CompleateAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> CreateLocalAsync(string name, long engineId, GameOptions options, Stream playerData, Stream gameData, CancellationToken cancellation = default);
    Task<DbGame> CreateRemoteAsync(string name, string serverAddress, GameOptions options, CancellationToken cancellation = default);
    Task<DbGame> PauseAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> StartAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> LockAsync(long gameId, CancellationToken cancellation = default);
    Task<DbGame> UnlockAsync(long gameId, CancellationToken cancellation = default);

    IQueryable<DbRegistration> GetRegistrations(long gameId);
    Task<DbRegistration> GetRegistrationAsync(long registrationId);
    Task<DbRegistration> RegisterAsync(long gameId, long userId, string name, CancellationToken cancellation = default);
    Task CancelRegistrationAsync(long registrationId);
}

public class GameRepository : IGameRepository {
    public GameRepository(IUnitOfWork unit, Database db) {
        this.unit = unit;
        this.db = db;
    }

    private readonly IUnitOfWork unit;
    private readonly Database db;

    public IQueryable<DbGame> Games => db.Games.AsQueryable();

    public async Task<DbGame> CreateLocalAsync(string name, long engineId, GameOptions options, Stream playerData, Stream gameData, CancellationToken cancellation) {
        await unit.BeginTransactionAsync(cancellation);

        var game = new DbGame {
            Name = name,
            Type = GameType.LOCAL,
            Status = GameStatus.NEW,
            CreatedAt = DateTimeOffset.UtcNow,
            Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
            EngineId = engineId,
            Options = options,
            NextTurnNumber = 1
        };

        await db.Games.AddAsync(game, cancellation);
        await unit.SaveChangesAsync(cancellation);

        ///// seed turn

        var seedTurn = await unit.Turns(game).AddTurnAsync(1, cancellation);
        seedTurn.PlayerData = await playerData.ReadAllBytesAsync(cancellation);
        seedTurn.GameData = await gameData.ReadAllBytesAsync(cancellation);

        /////

        await unit.SaveChangesAsync(cancellation);
        await unit.CommitTransactionAsync(cancellation);

        return game;
    }

    public async Task<DbGame> CreateRemoteAsync(string name, string serverAddress, GameOptions options, CancellationToken cancellation) {
        options.ServerAddress = serverAddress;

        var game = new DbGame {
            Name = name,
            Type = GameType.REMOTE,
            Status = GameStatus.NEW,
            CreatedAt = DateTimeOffset.UtcNow,
            Ruleset = await File.ReadAllTextAsync("data/ruleset.yaml"),
            Options = options
        };

        await db.Games.AddAsync(game, cancellation);

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

    public async Task<DbGame> GetOneAsync(long gameId)
        => await db.Games.FindAsync(gameId);

    public Task<DbGame> GetOneNoTrackingAsync(long gameId, CancellationToken cancellation = default)
        => Games.AsNoTracking().SingleOrDefaultAsync(x => x.Id == gameId, cancellation);

    public async Task<DbRegistration> RegisterAsync(long gameId, long userId, string name, CancellationToken cancellation) {
        var game = await GetOneNoTrackingAsync(gameId, cancellation);

        if (game.Type != GameType.LOCAL) {
            throw new RepositoryException("Local players can be added just to the local game.");
        }

        if (game.Status == GameStatus.COMPLEATED) {
            throw new RepositoryException("Game already ended.");
        }

        var player = await unit.Players(game)
            .GetOneByUserNoTrackingAsync(userId, cancellation);

        if (player != null) {
            throw new RepositoryException("Only one player allowed per user.");
        }

        var reg = await db.Registrations
            .AsNoTracking()
            .InGame(gameId)
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellation);

        if (reg != null) {
            throw new RepositoryException("There is an registration for this game.");
        }

        reg = new DbRegistration {
            GameId = gameId,
            UserId = userId,
            Name = name,
            Password = Guid.NewGuid().ToString("N") // there will be a random password
        };

        await db.Registrations.AddAsync(reg, cancellation);

        return reg;
    }

    public async Task CancelRegistrationAsync(long registrationId) {
        var reg = await GetRegistrationAsync(registrationId);

        db.Registrations.Remove(reg);
    }

    public IQueryable<DbRegistration> GetRegistrations(long gameId)
        => db.Registrations.InGame(gameId);

    public async Task<DbRegistration> GetRegistrationAsync(long registrationId)
        => await db.Registrations.FindAsync(registrationId);
}
