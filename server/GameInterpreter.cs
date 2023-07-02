namespace advisor;

using System;
using advisor.Persistence;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public readonly struct GameInterpreter<RT>
    where RT : struct, HasDatabase<RT>, HasUnitOfWork<RT>
{

    /// <summary>
    /// Interpret the Game DSL as an asynchronous effect.
    /// Will apply all write operations in a transaction.
    /// If interpretation fails, it will rollback any started transactions.
    /// </summary>
    public static Aff<RT, A> Interpret<A>(Mystcraft<A> dsl) =>
        _interpret(dsl)
        .IfFailAff<RT, A>(e =>
            UnitOfWork<RT>.rollback()
            .Map(_ => FailAff<A>(e))
            .Flatten()
        );

    private static Aff<RT, A> _interpret<A>(Mystcraft<A> dsl) => dsl switch
    {
        Mystcraft<A>.Return rt => SuccessAff(rt.Value),

        Mystcraft<A>.Create cr                => tran(createGame(cr)),
        Mystcraft<A>.ReadManyGames gm         =>      readManyGames(gm),
        Mystcraft<A>.ReadOneGame og           =>      readOneGame(og),
        Mystcraft<A>.Start st                 => tran(startGame(st)),
        Mystcraft<A>.Pause ps                 => tran(pauseGame(ps)),
        Mystcraft<A>.Stop sp                  => tran(stopGame(sp)),
        Mystcraft<A>.Delete dl                => tran(deleteGame(dl)),
        Mystcraft<A>.ReadOptions ro           =>      readOptions(ro),
        Mystcraft<A>.WriteSchedule ws         => tran(writeSchedule(ws)),
        Mystcraft<A>.WriteMap wm              => tran(writeMap(wm)),
        Mystcraft<A>.WritRuleset wr           => tran(writeRuleset(wr)),
        Mystcraft<A>.WritEngine we            => tran(writeEngine(we)),
        Mystcraft<A>.ReadManyRegistrations rg =>      readManyRegistrations(rg),
        Mystcraft<A>.ReadOneRegistration or   =>      readOneRegistration(or),
        Mystcraft<A>.RegisterPlayer rp        => tran(registerPlayer(rp)),
        Mystcraft<A>.RemoveRegistration rr    => tran(removeRegistration(rr)),
        Mystcraft<A>.ReadManyPlayers pl       =>      readManyPlayers(pl),
        Mystcraft<A>.ReadOnePlayer op         =>      readOnePlayer(op),
        Mystcraft<A>.QuitPlayer qp            => tran(quitPlayer(qp)),
        Mystcraft<A>.RunTurn rt               => tran(runTurn(rt)),
        Mystcraft<A>.ParseReport pr           =>      parseReport(pr),

        _ => FailAff<A>(Error.New(new NotSupportedException()))
    };

    private static Aff<RT, A> tran<A>(Aff<RT, A> action) =>
        from _1 in UnitOfWork<RT>.begin()
        from ret in action
        from _2 in UnitOfWork<RT>.commit()
        select ret;

    private static Aff<RT, A> createGame<A>(Mystcraft<A>.Create action) =>
        from games in Database<RT>.Games
        from game in Aff<RT, DbGame>(async rt => DbGame.CreateLocal(
            action.Name,
            action.Engine.Value,
            await action.Ruleset.ReadAllBytesAsync(rt.CancellationToken),
            new GameOptions
            {
                Map = action.Map,
                Schedule = action.Schedule.Cron,
                TimeZone = action.Schedule.TimeZone,
                StartAt = action.Schedule.StartAt,
                FinishAt = action.Schedule.FinishAt
            }
        ))
        from _1 in Database<RT>.add(game)
        from _2 in UnitOfWork<RT>.save()
        from ret in _interpret(action.Next(game))
        select ret;

    private static Aff<RT, A> readManyGames<A>(Mystcraft<A>.ReadManyGames gm) =>
        from games in Database<RT>.Games
        let selection = games.Where(gm.Predicate)
        from ret in _interpret(gm.Next(selection))
        select ret;

    private static Aff<RT, A> readOneGame<A>(Mystcraft<A>.ReadOneGame og) =>
        from games in Database<RT>.Games
        from oneGame in Aff<RT, DbGame>(async x => await games.Where(g => g.Id == og.Game.Value).FirstOrDefaultAsync(x.CancellationToken))
        from _ in guard(oneGame != null, Error.New($"Game {og.Game.Value} not found."))
        from ret in _interpret(og.Next(oneGame))
        select ret;

    private static Aff<RT, A> startGame<A>(Mystcraft<A>.Start st) =>
        from _ in guard(st.Game.Status == GameStatus.RUNNING, Error.New($"Game is already running."))
        from game in Eff(() => {
            st.Game.Status = GameStatus.RUNNING;
            return st.Game;
        })
        from ret in _interpret(st.Next(game))
        select ret;
}
