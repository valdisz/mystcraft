namespace advisor.Persistence;

public static class UnitOfWork<RT>
    where RT : struct, HasUnitOfWork<RT> {

    public static Aff<RT, Unit> begin() =>
        from ct in cancelToken<RT>()
        from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Begin(ct))
        select sc;

    public static Aff<RT, Unit> save() =>
        from ct in cancelToken<RT>()
        from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Save(ct))
        select sc;

    public static Aff<RT, Unit> commit() =>
        from ct in cancelToken<RT>()
        from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Commit(ct))
        select sc;

    public static Aff<RT, Unit> rollback() =>
        from ct in cancelToken<RT>()
        from sc in default(RT).UnitOfWorkEff.MapAsync(rt => rt.Rollback(ct))
        select sc;
}
