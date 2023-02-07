namespace advisor;

using System;
using System.Threading;

public interface IUnitOfWork : IAsyncDisposable, IDisposable {
    AsyncIO<Unit> BeginTransaction(CancellationToken cancellation = default);
    AsyncIO<Unit> CommitTransaction(CancellationToken cancellation = default);
    AsyncIO<Unit> RollbackTransaction(CancellationToken cancellation = default);
    AsyncIO<int> SaveChanges(CancellationToken cancellation = default);
}
