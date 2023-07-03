namespace advisor.Persistence;

using LanguageExt.Effects.Traits;

public static class UnitOfWork<RT>
    where RT : struct, HasUnitOfWork<RT>, HasCancel<RT> {

    public static Aff<RT, Unit> begin() =>
        from ct in cancelToken<RT>()
        from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Begin(ct))
        select sc;

    public static Aff<RT, Unit> save() =>
        from ct in cancelToken<RT>()
        from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Save(ct))
        select sc;

    public static Aff<RT, Unit> commit() {
        return AffMaybe<RT, Unit>(async rt => {
            var effect =
                from ct in cancelToken<RT>()
                from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Commit(ct))
                select sc;

            var result = await effect.Run(rt).ConfigureAwait(false);

            return result.Match(
                Succ: result => result.Match(
                    Right: FinSucc,
                    Left: FinFail<Unit>
                ),
                Fail: FinFail<Unit>
            );
        });
    }

    public static Aff<RT, Unit> rollback() =>
        from ct in cancelToken<RT>()
        from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Rollback(ct))
        select sc;
}
