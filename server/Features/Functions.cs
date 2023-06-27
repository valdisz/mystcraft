namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using System;
using advisor.Schema;

public static class Functions {
    public static Func<Option<T>, Result<T>> EnsurePresent<T>(string missing)
        => option => option
            .Select(Success)
            .Unwrap(() => Failure<T>(missing));

    private static AsyncIO<T> Validate<T>(this AsyncIO<Option<T>> self, string missing, Func<T, Result<T>> validator)
        => self
            .Bind(EnsurePresent<T>(missing))
            .Bind(validator);

    public static AsyncIO<DbGame> Validate(this AsyncIO<Option<DbGame>> self, Func<DbGame, Result<DbGame>> validator)
        => self.Validate("Game does not exist.", validator);

    public static AsyncIO<DbPlayer> Validate(this AsyncIO<Option<DbPlayer>> self, Func<DbPlayer, Result<DbPlayer>> validator)
        => self.Validate("Player does not exist.", validator);

    public static AsyncIO<DbTurn> Validate(this AsyncIO<Option<DbTurn>> self, Func<DbTurn, Result<DbTurn>> validator)
        => self.Validate("Turn does not exist.", validator);

    public static Task<R> RunWithRollback<T, R>(this AsyncIO<T> self, IUnitOfWork unitOfWork, Func<T, R> onSuccess, Func<Error, R> onFailure, CancellationToken cancellation)
        => self
            .Bind(value => unitOfWork.CommitTransaction(cancellation).Return(value))
            .Select(onSuccess)
            .OnFailure(() => unitOfWork.RollbackTransaction(cancellation))
            .Run()
            .Unwrap(onFailure);

    public static AsyncIO<DbGame> SaveAndReconcile(this AsyncIO<DbGame> self, IMediator mediator, IUnitOfWork unitOfWork, CancellationToken cancellation)
        => self
            .Bind(game => unitOfWork.SaveChanges(cancellation)
                .Bind(() => mediator.Reconcile(game.Id, cancellation))
                .Return(game)
            );

    public static AsyncIO<T> Mutate<T>(this IMediator self, IRequest<T> input, CancellationToken cancellation = default)
        where T: IMutationResult
        => AsyncEffect(() => self.Send(input, cancellation).AsResult());
}
