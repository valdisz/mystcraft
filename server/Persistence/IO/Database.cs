namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore;

public static class Database<RT>
    where RT : struct, HasDatabase<RT> {

    public static Eff<RT, DbSet<DbGameEngine>> GameEngines =>
        default(RT).DatabaseEff.Map(rt => rt.GameEngines);

    public static Eff<RT, DbSet<DbGame>> Games =>
        default(RT).DatabaseEff.Map(rt => rt.Games);

    public static Eff<RT, DbSet<DbPlayer>> Players =>
        default(RT).DatabaseEff.Map(rt => rt.Players);

    public static Eff<RT, DbSet<DbRegistration>> Registrations =>
        default(RT).DatabaseEff.Map(rt => rt.Registrations);

    public static Aff<RT, DbGame> add(DbGame game) =>
        from ct in cancelToken<RT>()
        from sc in default(RT).DatabaseEff.MapAsync(rt => rt.Add(game, ct))
        select sc;

    public static Aff<RT, DbGameEngine> add(DbGameEngine gameEngine) =>
        from ct in cancelToken<RT>()
        from sc in default(RT).DatabaseEff.MapAsync(rt => rt.Add(gameEngine, ct))
        select sc;
}
