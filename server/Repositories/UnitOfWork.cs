namespace advisor;

using System;
using System.Threading;
using System.Threading.Tasks;
using Persistence;

public interface IUnitOfWork : IAsyncDisposable, IDisposable {
    Database Database { get; }

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
        this.Database = db;

        this.Games = new GameRepository(this, db);
        this.Engines = new EnginesRepository(this, db);
    }

    private int txCounter = 0;
    private bool willRollback = false;
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction tx;

    public Database Database { get; }
    public IGameRepository Games { get; }
    public IEnginesRepository Engines { get; }

    public IPlayersRepository Players(DbGame game) {
        return new PlayersRepository(game, this, Database);
    }

    public async Task<IPlayersRepository> PlayersAsync(long gameId, CancellationToken cancellation) {
        var game = await Games.GetOneNoTrackingAsync(gameId, cancellation);
        return Players(game);
    }

    public ITurnsRepository Turns(DbGame game) => new TurnsRepository(game, this, Database);

    public async Task<ITurnsRepository> TurnsAsync(long gameId, CancellationToken cancellation) {
        var game = await Games.GetOneNoTrackingAsync(gameId, cancellation);
        return Turns(game);
    }

    public IPlayerRepository Player(DbPlayer player) => new PlayerRepository(player, this, Database);

    public async Task<IPlayerRepository> PlayerAsync(long playerId, CancellationToken cancellation) {
        var player = await Database.Players.FindAsync(playerId);
        return Player(player);
    }

    public async ValueTask<int> SaveChangesAsync(CancellationToken cancellation = default) {
        if (!Database.ChangeTracker.HasChanges()) {
            return 0;
        }

        return await Database.SaveChangesAsync();
    }

    public async ValueTask BeginTransactionAsync(CancellationToken cancellation = default) {
        if (txCounter == 0) {
            tx = await Database.Database.BeginTransactionAsync(cancellation);
            willRollback = false;
        }

        txCounter++;
    }

    public async ValueTask<bool> CommitTransactionAsync(CancellationToken cancellation = default) {
        if (!willRollback) {
            await SaveChangesAsync(cancellation);
        }

        if (txCounter == 0) {
            return true;
        }

        if (txCounter == 1) {
            if (willRollback) {
                await tx.RollbackAsync(cancellation);
            }
            else {
                await tx.CommitAsync(cancellation);
            }

            await tx.DisposeAsync();
            tx = null;
            txCounter = 0;
        }
        else {
            txCounter--;
        }

        return !willRollback;
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

        willRollback = true;
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
