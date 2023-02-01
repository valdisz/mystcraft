namespace advisor;

using System;
using System.Threading;
using System.Threading.Tasks;

public interface IUnitOfWork : IAsyncDisposable, IDisposable {
    Task BeginTransactionAsync(CancellationToken cancellation = default);
    Task<bool> CommitTransactionAsync(CancellationToken cancellation = default);
    Task RollbackTransactionAsync(CancellationToken cancellation = default);
    Task<int> SaveChangesAsync(CancellationToken cancellation = default);
}
