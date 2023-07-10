namespace advisor;

using System;
using advisor.Model;
using advisor.Persistence;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
/// Live interpreter for the basic operations that can be performed on a game.
/// </summary>
public readonly struct GameInterpreter<RT>
    where RT : struct, HasDatabase<RT>, HasUnitOfWork<RT>
{
    /// <summary>
    /// Interpret the Game DSL as an asynchronous effect.
    /// Will apply all write operations in a transaction.
    /// If operation fails, it will rollback any started transactions.
    /// </summary>
    public static Aff<RT, A> Interpret<A>(Mystcraft<A> dsl) =>
        _Interpret(dsl) | @catch(_Rollback<A>);

    private static Aff<RT, A> _Interpret<A>(Mystcraft<A> dsl) => dsl switch
    {
        Mystcraft<A>.Return rt => SuccessAff(rt.Value),

        Mystcraft<A>.CreateGameEngine cge     => _tran(CreateGameEngine(cge)),
        Mystcraft<A>.ReadManyGameEngines rge  =>       ReadManyGameEngines(rge),
        Mystcraft<A>.ReadOneGameEngine oge    =>       ReadOneGameEngine(oge),
        Mystcraft<A>.WriteOneGameEngine wge   => _tran(WriteOneGameEngine(wge)),
        Mystcraft<A>.DeleteGameEngine dge     =>       DeleteGameEngine(dge),
        Mystcraft<A>.CreateGame cr            => _tran(CreateGame(cr)),
        Mystcraft<A>.ReadManyGames gm         =>       ReadManyGames(gm),
        Mystcraft<A>.ReadOneGame og           =>       ReadOneGame(og),
        Mystcraft<A>.WriteOneGame wg          => _tran(WriteOneGame(wg)),
        Mystcraft<A>.Start st                 =>       StartGame(st),
        Mystcraft<A>.Pause ps                 =>       PauseGame(ps),
        Mystcraft<A>.Lock lk                  =>       LockGame(lk),
        Mystcraft<A>.Stop sp                  =>       StopGame(sp),
        Mystcraft<A>.DeleteGame dl                =>       DeleteGame(dl),
        // Mystcraft<A>.ReadOptions ro           =>       ReadOptions(ro),
        // Mystcraft<A>.WriteSchedule ws         => _tran(WriteSchedule(ws)),
        // Mystcraft<A>.WriteMap wm              => _tran(WriteMap(wm)),
        // Mystcraft<A>.WritRuleset wr           => _tran(WriteRuleset(wr)),
        // Mystcraft<A>.WritEngine we            => _tran(WriteEngine(we)),
        // Mystcraft<A>.ReadManyRegistrations rg =>       ReadManyRegistrations(rg),
        // Mystcraft<A>.ReadOneRegistration or   =>       ReadOneRegistration(or),
        // Mystcraft<A>.RegisterPlayer rp        => _tran(RegisterPlayer(rp)),
        // Mystcraft<A>.RemoveRegistration rr    => _tran(RemoveRegistration(rr)),
        // Mystcraft<A>.ReadManyPlayers pl       =>       ReadManyPlayers(pl),
        // Mystcraft<A>.ReadOnePlayer op         =>       ReadOnePlayer(op),
        // Mystcraft<A>.QuitPlayer qp            => _tran(QuitPlayer(qp)),
        // Mystcraft<A>.RunTurn rt               => _tran(RunTurn(rt)),
        // Mystcraft<A>.ParseReport pr           =>       ParseReport(pr),

        _ => FailAff<A>(E_OPERATION_NOT_SUPPORTED)
    };

    private static Aff<RT, A> _Rollback<A>(Error failure) =>
        from _ in UnitOfWork<RT>.rollback()
        from ret in FailAff<A>(failure)
        select ret;

    private static Aff<RT, A> _tran<A>(Aff<RT, A> action) =>
        from _1 in UnitOfWork<RT>.begin()
        from ret in action
        from _2 in UnitOfWork<RT>.commit()
        select ret;


    private static Eff<RT, A> _modify<A>(A value, Action<A> f) =>
        Eff(() => {
            f(value);
            return value;
        });

    private static Aff<RT, A> CreateGameEngine<A>(Mystcraft<A>.CreateGameEngine action) =>
        from ge in Database<RT>.add(DbGameEngine.New(
            action.Name,
            action.Contents,
            action.Ruleset
        ))
        from _ in UnitOfWork<RT>.save()
        from ret in _Interpret(action.Next(ge))
        select ret;

    private static Aff<RT, A> CreateGame<A>(Mystcraft<A>.CreateGame action) =>
        // TODO: improve this
        from game in Database<RT>.add(DbGame.New(
            action.Name,
            action.Engine.Value,
            new GameOptions
            {
                Map = action.Map,
                Schedule = action.Schedule.Cron,
                TimeZone = action.Schedule.TimeZone,
                StartAt = action.Period.StartAt.ToNullable(),
                FinishAt = action.Period.FinishAt.ToNullable()
            }
        ))
        from _ in UnitOfWork<RT>.save()
        from ret in _Interpret(action.Next(game))
        select ret;

    private static Aff<RT, A> ReadManyGameEngines<A>(Mystcraft<A>.ReadManyGameEngines action) =>
        from list in Database<RT>.GameEngines
        let selection = action.Predicate
            .Match(
                Some: p => list.Where(p),
                None: () => list
            )
            .OrderByDescending(g => g.CreatedAt)
        from ret in _Interpret(action.Next(selection))
        select ret;

    private static Aff<RT, A> ReadOneGameEngine<A>(Mystcraft<A>.ReadOneGameEngine action) =>
        from list in Database<RT>.GameEngines
        from item in list
            .AsNoTracking()
            .Where(g => g.Id == action.Engine.Value)
            .HeadOrFailAff<RT, DbGameEngine>(E_GAME_ENGINE_DOES_NOT_EXIST)
        from ret in _Interpret(action.Next(ReadOnly.New(item)))
        select ret;

    private static Aff<RT, A> WriteOneGameEngine<A>(Mystcraft<A>.WriteOneGameEngine action) =>
        from list in Database<RT>.GameEngines
        from item in list
            .Where(g => g.Id == action.Engine.Value)
            .HeadOrFailAff<RT, DbGameEngine>(E_GAME_ENGINE_DOES_NOT_EXIST)
        from ret in _Interpret(action.Next(item))
        select ret;

    private static Aff<RT, A> DeleteGameEngine<A>(Mystcraft<A>.DeleteGameEngine action) =>
        from games in Database<RT>.Games
        from hasGames in Aff<RT, bool>(rt => games.AnyAsync(g => g.EngineId == action.Engine.Id, rt.CancellationToken).ToValue())
        from _1 in guard(!hasGames, E_GAME_ENGINE_IS_IN_USE)
        from _2 in Database<RT>.delete(action.Engine)
        from _3 in UnitOfWork<RT>.save()
        from ret in _Interpret(action.Next(unit))
        select ret;

    private static Aff<RT, A> ReadManyGames<A>(Mystcraft<A>.ReadManyGames action) =>
        from list in Database<RT>.Games
        let selection = action.Predicate
            .Match(
                Some: p => list.Where(p),
                None: () => list
            )
            .OrderByDescending(g => g.CreatedAt)
        from ret in _Interpret(action.Next(selection))
        select ret;

    private static Aff<RT, A> ReadOneGame<A>(Mystcraft<A>.ReadOneGame og) =>
        from games in Database<RT>.Games
        from game in games
            .AsNoTracking()
            .Where(g => g.Id == og.Game.Value)
            .HeadOrFailAff<RT, DbGame>(E_GAME_DOES_NOT_EXIST)
        from ret in _Interpret(og.Next(ReadOnly.New(game)))
        select ret;

    private static Aff<RT, A> WriteOneGame<A>(Mystcraft<A>.WriteOneGame wg) =>
        from games in Database<RT>.Games
        from game in games
            .Where(g => g.Id == wg.Game.Value)
            .HeadOrFailAff<RT, DbGame>(E_GAME_DOES_NOT_EXIST)
        from ret in _Interpret(wg.Next(game))
        select ret;

    private static Either<Error, DbGame> CanStartGame(DbGame game) => game switch {
        { Status: GameStatus.RUNNING } => Left(E_GAME_ALREADY_RUNNING),
        _                              => Right(game)
    };

    private static Aff<RT, A> StartGame<A>(Mystcraft<A>.Start st) =>
        from game in CanStartGame(st.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.RUNNING)
        from ret in _Interpret(st.Next(game))
        select ret;

    private static Either<Error, DbGame> CanPauseGame(DbGame game) => game switch {
        { Status: GameStatus.NEW }    => Left(E_GAME_MUST_BE_RUNNING),
        { Status: GameStatus.PAUSED } => Left(E_GAME_ALREADY_PAUSED),
        { Status: GameStatus.LOCKED } => Left(E_GAME_LOCKED),
        { Status: GameStatus.STOPED } => Left(E_GAME_STOPED),
        _                             => Right(game)
    };

    private static Aff<RT, A> PauseGame<A>(Mystcraft<A>.Pause ps) =>
        from game in CanPauseGame(ps.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.RUNNING)
        from ret in _Interpret(ps.Next(game))
        select ret;

    private static Either<Error, DbGame> CanLockGame(DbGame game) => game switch {
        { Status: GameStatus.NEW }    => Left(E_GAME_MUST_BE_RUNNING),
        { Status: GameStatus.PAUSED } => Left(E_GAME_PAUSED),
        { Status: GameStatus.LOCKED } => Left(E_GAME_ALREADY_LOCKED),
        { Status: GameStatus.STOPED } => Left(E_GAME_STOPED),
        _                             => Right(game)
    };

    private static Aff<RT, A> LockGame<A>(Mystcraft<A>.Lock lk) =>
        from game in CanLockGame(lk.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.LOCKED)
        from ret in _Interpret(lk.Next(game))
        select ret;

    private static Either<Error, DbGame> CanStopGame(DbGame game) => game switch {
        { Status: GameStatus.STOPED } => Left(E_GAME_ALREADY_STOPED),
        _                             => Right(game)
    };

    private static Aff<RT, A> StopGame<A>(Mystcraft<A>.Stop sp) =>
        from game in CanStopGame(sp.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.STOPED)
        from ret in _Interpret(sp.Next(game))
        select ret;

    private static Aff<RT, A> DeleteGame<A>(Mystcraft<A>.DeleteGame dl) =>
        from games in Database<RT>.Games
        from _1 in Eff<RT, EntityEntry<DbGame>>(x => games.Remove(dl.Game))
        from _2 in UnitOfWork<RT>.save()
        from ret in _Interpret(dl.Next(unit))
        select ret;
}
