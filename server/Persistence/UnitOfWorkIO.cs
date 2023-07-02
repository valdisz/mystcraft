namespace advisor.Persistence;

using System.Threading;
using System.Threading.Tasks;
using LanguageExt.Effects.Traits;

public interface HasUnitOfWork<RT>: HasCancel<RT>
    where RT : struct, HasUnitOfWork<RT>, HasCancel<RT> {

    Eff<RT, UnitOfWorkIO> UnitOfWorkEff { get; }
}

public interface UnitOfWorkIO {
    ValueTask<Unit> Begin(CancellationToken ct);
    ValueTask<Unit> Save(CancellationToken ct);
    ValueTask<Unit> Commit(CancellationToken ct);
    ValueTask<Unit> Rollback(CancellationToken ct);
}
