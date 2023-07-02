namespace advisor;

using System;
using System.IO;
using System.Collections.Generic;
using advisor.Persistence;
using System.Linq.Expressions;
using System.Linq;
using advisor.Model;

/// <summary>
/// IO independent operations interacting with the game entity. It will hold everything that can be done with the game entity
/// like creating a new game, modifying it, etc.
/// </summary>
public static class Mystcraft {
    /// <summary>
    /// Return the provided value.
    /// </summary>
    public static Mystcraft<A> Return<A>(A value) => new Mystcraft<A>.Return(value);

    /// <summary>
    /// Create a new game.
    /// </summary>
    public static Mystcraft<DbGame> Create(string Name, EngineId Engine, Stream Ruleset, List<MapLevel> Map, GameSchedule Schedule) => new Mystcraft<DbGame>.Create(Name, Engine, Ruleset, Map, Schedule, Return);

    /// <summary>
    /// Get a list of all games.
    /// </summary>
    public static Mystcraft<IQueryable<DbGame>> ReadManyGames() => ReadManyGames(_ => true);

    /// <summary>
    /// Get a list of all games matching the predicate.
    /// </summary>
    public static Mystcraft<IQueryable<DbGame>> ReadManyGames(Expression<Func<DbGame, bool>> Predicate) => new Mystcraft<IQueryable<DbGame>>.ReadManyGames(Predicate, Return);

    /// <summary>
    /// Get a single game by ID.
    /// </summary>
    public static Mystcraft<DbGame> ReadOneGame(GameId Game) => new Mystcraft<DbGame>.ReadOneGame(Game, Return);

    /// <summary>
    /// Start or resume a game.
    /// </summary>
    public static Mystcraft<DbGame> Start(DbGame Game) => new Mystcraft<DbGame>.Start(Game, Return);

    /// <summary>
    /// Pause a game unless already running.
    /// Paused game will accept new player registrations but will not run the next turn.
    /// </summary>
    public static Mystcraft<DbGame> Pause(GameId Game) => new Mystcraft<DbGame>.Pause(Game, Return);

    /// <summary>
    /// Stop a game unless already stopped.
    /// Stopped game will not accept new player registrations and will not run the next turn.
    /// </summary>
    public static Mystcraft<DbGame> Stop(GameId Game) => new Mystcraft<DbGame>.Stop(Game, Return);

    /// <summary>
    /// Delete a game with all its data.
    /// Cannot be undone and not data will be recoverable.
    /// </summary>
    public static Mystcraft<GameId> Delete(GameId Game) => new Mystcraft<GameId>.Delete(Game, Return);

    /// <summary>
    /// Read current game options.
    /// </summary>
    public static Mystcraft<GameOptions> ReadOptions(GameId Game) => new Mystcraft<GameOptions>.ReadOptions(Game, Return);

    /// <summary>
    /// Change game schedule.
    /// </summary>
    public static Mystcraft<Unit> WriteSchedule(GameId Game, GameSchedule Schedule) => new Mystcraft<Unit>.WriteSchedule(Game, Schedule, Return);

    /// <summary>
    /// Change game map settings.
    /// </summary>
    public static Mystcraft<Unit> WriteMap(GameId Game, List<MapLevel> Map) => new Mystcraft<Unit>.WriteMap(Game, Map, Return);

    /// <summary>
    /// Change game ruleset.
    /// </summary>
    public static Mystcraft<Unit> WritRuleset(GameId Game, Stream Ruleset) => new Mystcraft<Unit>.WritRuleset(Game, Ruleset, Return);

    /// <summary>
    /// Change game engine.
    /// </summary>
    public static Mystcraft<Unit> WritEngine(GameId Game, EngineId Engine) => new Mystcraft<Unit>.WritEngine(Game, Engine, Return);

    /// <summary>
    /// Get a list of all player registrations.
    /// </summary>
    public static Mystcraft<IQueryable<DbRegistration>> ReadManyRegistrations(GameId Game, Expression<Func<DbRegistration, bool>> Predicate) => new Mystcraft<IQueryable<DbRegistration>>.ReadManyRegistrations(Game, Predicate, Return);

    /// <summary>
    /// Get a single player registration.
    /// </summary>
    public static Mystcraft<DbRegistration> ReadOneRegistration(RegistrationId Registration) => new Mystcraft<DbRegistration>.ReadOneRegistration(Registration, Return);

    /// <summary>
    /// Register a player into list of new factions to be added into the game in the next game turn.
    /// </summary>
    public static Mystcraft<DbRegistration> RegisterPlayer(GameId Game, string Name, string Password) => new Mystcraft<DbRegistration>.RegisterPlayer(Game, Name, Password, Return);

    /// <summary>
    /// Remove a player registration from the game.
    /// </summary>
    public static Mystcraft<RegistrationId> RemoveRegistration(RegistrationId Registration) => new Mystcraft<RegistrationId>.RemoveRegistration(Registration, Return);

    /// <summary>
    /// Get a list of all players.
    /// </summary>
    public static Mystcraft<IQueryable<DbPlayer>> ReadManyPlayers(GameId Game, bool IncludeQuit, Expression<Func<DbPlayer, bool>> Predicate) => new Mystcraft<IQueryable<DbPlayer>>.ReadManyPlayers(Game, IncludeQuit, Predicate, Return);

    /// <summary>
    /// Get a single player.
    /// </summary>
    public static Mystcraft<DbPlayer> ReadOnePlayer(PlayerId Player) => new Mystcraft<DbPlayer>.ReadOnePlayer(Player, Return);

    /// <summary>
    /// Quite a player faction from the game in the next game turn.
    /// </summary>
    public static Mystcraft<PlayerId> QuitPlayer(PlayerId Player) => new Mystcraft<PlayerId>.QuitPlayer(Player, Return);

    /// <summary>
    /// Run the next game turn using the Game Engine.
    /// </summary>
    public static Mystcraft<DbTurn> RunTurn(GameId Game) => new Mystcraft<DbTurn>.RunTurn(Game, Return);

    /// <summary>
    /// Parse the turn report.
    /// </summary>
    public static Mystcraft<JReport> ParseReport(Stream Report) => new Mystcraft<JReport>.ParseReport(Report, Return);
}

public static class Sandbox {
    public static async void Foo() {
        var addNewRegistration =
            from game in Mystcraft.ReadOneGame(new GameId(1))
            from reg in Mystcraft.RegisterPlayer(new GameId(game.Id), "name", "password")
            select reg;

        Runtime rt = Runtime.New(null);

        (await GameInterpreter<Runtime>
            .Interpret(addNewRegistration, rt))
            .Match(
                Succ: r => Console.WriteLine(r),
                Fail: e => Console.WriteLine(e)
            );
    }
}

/// <summary>
/// Game schedule.
/// </summary>
public record GameSchedule(
    string Cron,
    string TimeZone,
    DateTimeOffset? StartAt,
    DateTimeOffset? FinishAt
) {
    public static GameSchedule New(string cron, string timeZone, DateTimeOffset? startAt, DateTimeOffset? finishAt) =>
        new GameSchedule(cron, timeZone, startAt, finishAt);
}

public static class MystcraftExtensions
{
    public static Mystcraft<B> Bind<A, B>(this Mystcraft<A> ma, Func<A, Mystcraft<B>> f) => ma switch {
        Mystcraft<A>.Return rt                => f(rt.Value),

        Mystcraft<A>.Create cr                => new Mystcraft<B>.Create(cr.Name, cr.Engine, cr.Ruleset, cr.Map, cr.Schedule, n => cr.Next(n).Bind(f)),
        Mystcraft<A>.ReadManyGames gm         => new Mystcraft<B>.ReadManyGames(gm.Predicate, n => gm.Next(n).Bind(f)),
        Mystcraft<A>.ReadOneGame og           => new Mystcraft<B>.ReadOneGame(og.Game, n => og.Next(n).Bind(f)),
        Mystcraft<A>.Start st                 => new Mystcraft<B>.Start(st.Game, n => st.Next(n).Bind(f)),
        Mystcraft<A>.Pause ps                 => new Mystcraft<B>.Pause(ps.Game, n => ps.Next(n).Bind(f)),
        Mystcraft<A>.Stop sp                  => new Mystcraft<B>.Stop(sp.Game, n => sp.Next(n).Bind(f)),
        Mystcraft<A>.Delete dl                => new Mystcraft<B>.Delete(dl.Game, n => dl.Next(n).Bind(f)),
        Mystcraft<A>.ReadOptions ro           => new Mystcraft<B>.ReadOptions(ro.Game, n => ro.Next(n).Bind(f)),
        Mystcraft<A>.WriteSchedule ws         => new Mystcraft<B>.WriteSchedule(ws.Game, ws.Schedule, n => ws.Next(n).Bind(f)),
        Mystcraft<A>.WriteMap wm              => new Mystcraft<B>.WriteMap(wm.Game, wm.Map, n => wm.Next(n).Bind(f)),
        Mystcraft<A>.WritRuleset wr           => new Mystcraft<B>.WritRuleset(wr.Game, wr.Ruleset, n => wr.Next(n).Bind(f)),
        Mystcraft<A>.WritEngine we            => new Mystcraft<B>.WritEngine(we.Game, we.Engine, n => we.Next(n).Bind(f)),
        Mystcraft<A>.ReadManyRegistrations rg => new Mystcraft<B>.ReadManyRegistrations(rg.Game, rg.Predicate, n => rg.Next(n).Bind(f)),
        Mystcraft<A>.ReadOneRegistration or   => new Mystcraft<B>.ReadOneRegistration(or.Registration, n => or.Next(n).Bind(f)),
        Mystcraft<A>.RegisterPlayer rp        => new Mystcraft<B>.RegisterPlayer(rp.Game, rp.Name, rp.Password, n => rp.Next(n).Bind(f)),
        Mystcraft<A>.RemoveRegistration rr    => new Mystcraft<B>.RemoveRegistration(rr.Registration, n => rr.Next(n).Bind(f)),
        Mystcraft<A>.ReadManyPlayers pl       => new Mystcraft<B>.ReadManyPlayers(pl.Game, pl.IncludeQuit, pl.Predicate, n => pl.Next(n).Bind(f)),
        Mystcraft<A>.ReadOnePlayer op         => new Mystcraft<B>.ReadOnePlayer(op.Player, n => op.Next(n).Bind(f)),
        Mystcraft<A>.QuitPlayer qp            => new Mystcraft<B>.QuitPlayer(qp.Player, n => qp.Next(n).Bind(f)),
        Mystcraft<A>.RunTurn rt               => new Mystcraft<B>.RunTurn(rt.Game, n => rt.Next(n).Bind(f)),
        Mystcraft<A>.ParseReport pr           => new Mystcraft<B>.ParseReport(pr.Report, n => pr.Next(n).Bind(f)),

        _ => throw new NotSupportedException()
    };


    public static Mystcraft<B> Map<A, B>(this Mystcraft<A> ma, Func<A, B> f) =>
        ma.Bind(a => Mystcraft.Return(f(a)));

    public static Mystcraft<B> Select<A, B>(this Mystcraft<A> ma, Func<A, B> f) =>
        ma.Bind(a => Mystcraft.Return(f(a)));

    public static Mystcraft<C> SelectMany<A, B, C>(this Mystcraft<A> ma, Func<A, Mystcraft<B>> bind, Func<A, B, C> project) =>
        ma.Bind(a => bind(a).Select(b => project(a, b)));
}

/// <summary>
/// Represents a game ID.
/// </summary>
public sealed class GameId : NewType<GameId, long> {
    public GameId(long value) : base(value) { }
}

/// <summary>
/// Represents an game engine ID.
/// </summary>
public sealed class EngineId : NewType<EngineId, long> {
    public EngineId(long value) : base(value) { }
}

/// <summary>
/// Represents a player registration ID.
/// </summary>
public sealed class RegistrationId : NewType<RegistrationId, long> {
    public RegistrationId(long value) : base(value) { }
}

/// <summary>
/// Represents a player ID.
/// </summary>
public sealed class PlayerId : NewType<PlayerId, long> {
    public PlayerId(long value) : base(value) { }
}

public sealed class TurnNumber : NewType<TurnNumber, long> {
    public TurnNumber(long value) : base(value) { }
}

/// <summary>
/// Base class for the independent operations interacting with the game entity.
/// </summary>
public abstract record Mystcraft<A> {
    /// <summary>
    /// Identity type - simply returns the value provided.
    /// </summary>
    public sealed record Return(A Value) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// Game creation and retrieval

    /// <summary>
    /// Create a new game.
    /// </summary>
    public sealed record Create(string Name, EngineId Engine, Stream Ruleset, List<MapLevel> Map, GameSchedule Schedule, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a list of all games.
    /// </summary>
    public sealed record ReadManyGames(Expression<Func<DbGame, bool>> Predicate, Func<IQueryable<DbGame>, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a single game.
    /// </summary>
    public sealed record ReadOneGame(GameId Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// State transitions

    /// <summary>
    /// Represents operation that starts a game.
    /// </summary>
    public sealed record Start(DbGame Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that pauses a game.
    /// </summary>
    public sealed record Pause(GameId Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that fully stops a game.
    /// </summary>
    public sealed record Stop(GameId Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that deletes a game.
    /// </summary>
    public sealed record Delete(GameId Game, Func<GameId, Mystcraft<A>> Next) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// Game options

    /// <summary>
    /// Represents operation that reads game options.
    /// </summary>
    public sealed record ReadOptions(GameId Game, Func<GameOptions, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that changes game schedule.
    /// </summary>
    public sealed record WriteSchedule(GameId Game, GameSchedule Schedule, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that changes game map settings.
    /// </summary>
    public sealed record WriteMap(GameId Game, List<MapLevel> Map, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that changes game ruleset.
    /// </summary>
    public sealed record WritRuleset(GameId Game, Stream Ruleset, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that changes game engine.
    /// </summary>
    public sealed record WritEngine(GameId Game, EngineId Engine, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// Player registration operations

    /// <summary>
    /// Represents operation that gets a list of all player registrations.
    /// </summary>
    public sealed record ReadManyRegistrations(GameId Game, Expression<Func<DbRegistration, bool>> Predicate, Func<IQueryable<DbRegistration>, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a single player registration.
    /// </summary>
    public sealed record ReadOneRegistration(RegistrationId Registration, Func<DbRegistration, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that registers a player into list of new factions to be added into the game in the next game turn.
    /// </summary>
    public sealed record RegisterPlayer(GameId Game, string Name, string Password, Func<DbRegistration, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that removes a player registration from the game.
    /// </summary>
    public sealed record RemoveRegistration(RegistrationId Registration, Func<RegistrationId, Mystcraft<A>> Next) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// Player operations

    /// <summary>
    /// Represents operation that gets a list of all players.
    /// </summary>
    public sealed record ReadManyPlayers(GameId Game, bool IncludeQuit, Expression<Func<DbPlayer, bool>> Predicate, Func<IQueryable<DbPlayer>, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a single player.
    /// </summary>
    public sealed record ReadOnePlayer(PlayerId Player, Func<DbPlayer, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that quites a player faction from the game in the next game turn.
    /// </summary>
    public sealed record QuitPlayer(PlayerId Player, Func<PlayerId, Mystcraft<A>> Next) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// Game turn operations

    /// <summary>
    /// Represents operation that runs the next game turn using the Game Engine.
    /// </summary>
    public sealed record RunTurn(GameId Game, Func<DbTurn, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that parses the turn report.
    /// </summary>
    public sealed record ParseReport(Stream Report, Func<JReport, Mystcraft<A>> Next) : Mystcraft<A>;

    // public sealed record ParseTurn(PlayerId Player, TurnNumber Turn, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    // public sealed record MergeTurn(PlayerId Player, TurnNumber Turn, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    // public sealed record CollectTurnStatistics(PlayerId Player, TurnNumber Turn, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;
}
