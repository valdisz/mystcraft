namespace advisor;

using System;
using System.Threading;
using System.Threading.Tasks;
using Persistence;

public interface IUnitOfWork : IAsyncDisposable, IDisposable {
    IGameRepository Games { get; }
    IEnginesRepository Engines { get; }

    IPlayersRepository Players(DbGame game);
    Task<IPlayersRepository> PlayersAsync(long gameId, CancellationToken cancellation = default);

    ITurnsRepository Turns(DbGame game);
    Task<ITurnsRepository> TurnsAsync(long gameId, CancellationToken cancellation = default);

    IPlayerRepository Player(DbPlayer player);
    Task<IPlayerRepository> PlayerAsync(long playerId, CancellationToken cancellation = default);

    ValueTask BeginTransactionAsync(CancellationToken cancellation = default);
    ValueTask<bool> CommitTransactionAsync(CancellationToken cancellation = default);
    ValueTask RollbackTransactionAsync(CancellationToken cancellation = default);
    ValueTask<int> SaveChangesAsync(CancellationToken cancellation = default);
}

public class UnitOfWork : IUnitOfWork {
    public UnitOfWork(Database db) {
        this.db = db;

        this.Games = new GameRepository(this, db);
        this.Engines = new EnginesRepository(this, db);
    }

    private readonly Database db;

    private int txCounter = 0;
    private bool txRollback = false;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx;

    public IGameRepository Games { get; }
    public IEnginesRepository Engines { get; }

    public IPlayersRepository Players(DbGame game) {
        return new PlayersRepository(game, this, db);
    }

    public async Task<IPlayersRepository> PlayersAsync(long gameId, CancellationToken cancellation) {
        var game = await Games.GetOneNoTrackingAsync(gameId, cancellation);
        return Players(game);
    }

    public ITurnsRepository Turns(DbGame game) => new TurnsRepository(game, this, db);

    public async Task<ITurnsRepository> TurnsAsync(long gameId, CancellationToken cancellation) {
        var game = await Games.GetOneNoTrackingAsync(gameId, cancellation);
        return Turns(game);
    }

    public IPlayerRepository Player(DbPlayer player) => new PlayerRepository(player, this, db);

    public async Task<IPlayerRepository> PlayerAsync(long playerId, CancellationToken cancellation) {
        var player = await db.Players.FindAsync(playerId);
        return Player(player);
    }

    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellation = default) {
        if (!db.ChangeTracker.HasChanges()) {
            return 0;
        }

        return await db.SaveChangesAsync();
    }

    public async ValueTask BeginTransactionAsync(CancellationToken cancellation = default) {
        if (txCounter == 0) {
            tx = await db.Database.BeginTransactionAsync(cancellation);
            txRollback = false;
        }

        txCounter++;
    }

    public async ValueTask<bool> CommitTransactionAsync(CancellationToken cancellation = default) {
        if (txCounter == 0) {
            return true;
        }

        if (txCounter == 1) {
            if (txRollback) {
                await tx.RollbackAsync(cancellation);
            }
            else {
                await SaveChangesAsync(cancellation);
                await tx.CommitAsync(cancellation);
            }

            await tx.DisposeAsync();
            tx = null;
            txCounter = 0;
        }
        else {
            txCounter--;
        }

        return !txRollback;
    }

    public async ValueTask RollbackTransactionAsync(CancellationToken cancellation = default) {
        if (txCounter == 0) {
            return;
        }

        if (txCounter == 1) {
            await tx.RollbackAsync(cancellation);
            await tx.DisposeAsync();
            tx = null;
            txCounter = 0;
        }
        else {
            txCounter--;
        }

        txRollback = true;
    }

    public async ValueTask DisposeAsync() {
        if (txCounter == 0) {
            return;
        }

        await tx.RollbackAsync();
        await tx.DisposeAsync();
    }

    public void Dispose() {
        if (txCounter == 0) {
            return;
        }

        tx.Rollback();
        tx.Dispose();
    }
}
