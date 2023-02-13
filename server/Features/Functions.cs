namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using System;

public static class Functions {
    public static AsyncIO<DbGame> GetOneGame(this IGameRepository repo, long gameId, Func<DbGame, Result<DbGame>> validate, CancellationToken cancellation)
        => repo.GetOneGame(gameId, cancellation: cancellation)
            .Select(EnsurePresent<DbGame>("Game does not exist."))
            .Select(validate);

    public static AsyncIO<DbPlayer> GetOnePlayer(this ISpecializedPlayerRepository repo, long playerId, Func<DbPlayer, Result<DbPlayer>> validate, CancellationToken cancellation)
        => repo.GetOnePlayer(playerId, cancellation: cancellation)
            .Select(EnsurePresent<DbPlayer>("Player does not exist."))
            .Select(validate);

    public static Func<AsyncIO<T>, Task<R>> RunWithRollback<T, R>(this IUnitOfWork unitOfWork, Func<T, R> onSuccess, Func<Error, R> onFailure, CancellationToken cancellation)
        => (AsyncIO<T> self) => self
            .Bind(value => unitOfWork.CommitTransaction(cancellation).Return(value))
            .Select(onSuccess)
            .OnFailure(_ => unitOfWork.RollbackTransaction(cancellation))
            .Run()
            .Unwrap(onFailure);

    public static IO<DbGame> UpdateGame(this IGameRepository repo, DbGame game, Action<DbGame> onUpdate)
        => () => {
            onUpdate(game);
            return repo.Update(game)();
        };

    public static AsyncIO<ReconcileResult> Reconcile(long gameId, IMediator mediator, CancellationToken cancellation)
        => AsyncEffect(() => mediator.Send(new Reconcile(gameId), cancellation).AsResult());

    public static Func<Option<T>, Result<T>> EnsurePresent<T>(string missing)
        => option => option.Select(Success).Unwrap(() => Failure<T>(missing));
}
