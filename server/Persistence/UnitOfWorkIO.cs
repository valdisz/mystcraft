namespace advisor.Persistence;

using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Effects.Traits;

public interface HasUnitOfWork<RT>: HasCancel<RT>
    where RT : struct, HasUnitOfWork<RT>, HasCancel<RT> {

    Eff<RT, UnitOfWork> UnitOfWorkEff { get; }
}

public interface UnitOfWork {
    Database Database { get; }

    ValueTask<Unit> Begin(CancellationToken ct);
    ValueTask<Unit> Save(CancellationToken ct);
    ValueTask<Either<Error, Unit>> Commit(CancellationToken ct);
    ValueTask<Unit> Rollback(CancellationToken ct);
}
