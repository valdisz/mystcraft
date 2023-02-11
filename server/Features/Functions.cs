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
            .Select(onSuccess)
            .OnFailure(_ => unitOfWork.RollbackTransaction(cancellation))
            .Run()
            .Unwrap(onFailure);

    public static IO<DbGame> UpdateGame(this IGameRepository repo, DbGame game, Action<DbGame> update)
        => () => {
            update(game);
            repo.Update(game);
            return Success(game);
        };

    public static AsyncIO<advisor.Unit> Reconcile(long gameId, IMediator mediator, CancellationToken cancellation)
        => Effect(() => mediator.Send(new Reconcile(gameId), cancellation))
            .Select(result => result.IsSuccess ? Success(unit) : Failure<advisor.Unit>(result.Error));

    public static Func<Option<T>, Result<T>> EnsurePresent<T>(string missing)
        => option => option.Select(Success).Unwrap(() => Failure<T>(missing));
}
