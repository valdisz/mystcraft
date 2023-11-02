namespace advisor.Model;

using System;
using System.IO;
using System.Collections.Generic;
using advisor.Persistence;
using System.Linq.Expressions;
using System.Linq;

/// <summary>
/// IO independent operations interacting with the game entity. It will hold everything that can be done with the game entity
/// like creating a new game, modifying it, etc.
/// </summary>
public static class Mystcraft {
    /// <summary>
    /// Return the provided value.
    /// </summary>
    public static Mystcraft<A> Return<A>(A value) =>
        new Mystcraft<A>.Return(value);

    /// <summary>
    /// Create a new game engine.
    /// </summary>
    public static Mystcraft<DbGameEngine> CreateGameEngine(string Name, string Description, byte[] Engine, byte[] Ruleset) =>
        new Mystcraft<DbGameEngine>.CreateGameEngine(Name, Description, Engine, Ruleset, Return);

    /// <summary>
    /// Create a new remote game engine.
    /// </summary>
    public static Mystcraft<DbGameEngine> CreateGameEngineRemote(string Name, string Description, string Api, string Url, string Options) =>
        new Mystcraft<DbGameEngine>.CreateGameEngineRemote(Name, Description, Api, Url, Options, Return);

    /// <summary>
    /// Get a list of all game engines.
    /// </summary>
    public static Mystcraft<IOrderedQueryable<DbGameEngine>> ReadManyGameEngines() =>
        new Mystcraft<IOrderedQueryable<DbGameEngine>>.ReadManyGameEngines(None, Return);

    /// <summary>
    /// Get a list of all game engines matching the predicate.
    /// </summary>
    public static Mystcraft<IOrderedQueryable<DbGameEngine>> ReadManyGameEngines(Expression<Func<DbGameEngine, bool>> Predicate) =>
        new Mystcraft<IOrderedQueryable<DbGameEngine>>.ReadManyGameEngines(Some(Predicate), Return);

    /// <summary>
    /// Get a single game engine by ID in a read-only mode.
    /// </summary>
    public static Mystcraft<ReadOnly<DbGameEngine>> ReadOneGameEngine(GameEngineId Game) =>
        new Mystcraft<ReadOnly<DbGameEngine>>.ReadOneGameEngine(Game, Return);

    /// <summary>
    /// Get a single game engine by ID prepared for modification.
    /// </summary>
    public static Mystcraft<DbGameEngine> WriteOneGameEngine(GameEngineId Game) =>
        new Mystcraft<DbGameEngine>.WriteOneGameEngine(Game, Return);

    /// <summary>
    /// Delete a game engine.
    /// </summary>
    public static Mystcraft<Unit> DeleteGameEngine(DbGameEngine Engine) =>
        new Mystcraft<Unit>.DeleteGameEngine(Engine, Return);

    /// <summary>
    /// Create a new remote game.
    /// </summary>
    public static Mystcraft<DbGame> CreateGameRemote(string Name, GameEngineId Engine, List<MapLevel> Levels, GameSchedule Schedule, GamePeriod Period) =>
        new Mystcraft<DbGame>.CreateGameRemote(Name, Engine, Levels, Schedule, Period, Return);


    /// <summary>
    /// Create a new local game.
    /// </summary>
    public static Mystcraft<DbGame> CreateGameLocal(string Name, GameEngineId Engine, List<MapLevel> Levels, GameSchedule Schedule, GamePeriod Period, byte[] GameIn, byte[] PlayersIn) =>
        new Mystcraft<DbGame>.CreateGameLocal(Name, Engine, Levels, Schedule, Period, GameIn, PlayersIn, Return);

    /// <summary>
    /// Get a list of all games.
    /// </summary>
    public static Mystcraft<IOrderedQueryable<DbGame>> ReadManyGames() =>
        new Mystcraft<IOrderedQueryable<DbGame>>.ReadManyGames(None, Return);

    /// <summary>
    /// Get a list of all games matching the predicate.
    /// </summary>
    public static Mystcraft<IOrderedQueryable<DbGame>> ReadManyGames(Expression<Func<DbGame, bool>> Predicate) =>
        new Mystcraft<IOrderedQueryable<DbGame>>.ReadManyGames(Some(Predicate), Return);

    /// <summary>
    /// Get a single game by ID in a read-only mode.
    /// </summary>
    public static Mystcraft<ReadOnly<DbGame>> ReadOneGame(GameId Game) =>
        new Mystcraft<ReadOnly<DbGame>>.ReadOneGame(Game, Return);

    /// <summary>
    /// Get a single game by ID prepared for modification.
    /// </summary>
    public static Mystcraft<DbGame> WriteOneGame(GameId Game) =>
        new Mystcraft<DbGame>.WriteOneGame(Game, Return);

    /// <summary>
    /// Start or resume a game.
    /// </summary>
    public static Mystcraft<DbGame> Start(DbGame Game) =>
        new Mystcraft<DbGame>.Start(Game, Return);

    /// <summary>
    /// Pause a game unless already running.
    /// Paused game will accept new player registrations but will not run the next turn.
    /// </summary>
    public static Mystcraft<DbGame> Pause(DbGame Game) =>
        new Mystcraft<DbGame>.Pause(Game, Return);

    /// <summary>
    /// Lock a game unless already locked and prevent new player registrations and order updates.
    /// </summary>
    public static Mystcraft<DbGame> Lock(DbGame Game) =>
        new Mystcraft<DbGame>.Lock(Game, Return);

    /// <summary>
    /// Stop a game unless already stopped.
    /// Stopped game will not accept new player registrations and will not run the next turn.
    /// </summary>
    public static Mystcraft<DbGame> Stop(DbGame Game) =>
        new Mystcraft<DbGame>.Stop(Game, Return);

    /// <summary>
    /// Delete a game with all its data.
    /// Cannot be undone and not data will be recoverable.
    /// </summary>
    public static Mystcraft<Unit> DeleteGame(DbGame Game) =>
        new Mystcraft<Unit>.DeleteGame(Game, Return);

    /// <summary>
    /// Read current game options.
    /// </summary>
    public static Mystcraft<GameOptions> ReadOptions(GameId Game) =>
        new Mystcraft<GameOptions>.ReadOptions(Game, Return);

    /// <summary>
    /// Change game schedule.
    /// </summary>
    public static Mystcraft<Unit> WriteSchedule(GameId Game, GameSchedule Schedule) =>
        new Mystcraft<Unit>.WriteSchedule(Game, Schedule, Return);

    /// <summary>
    /// Change game map settings.
    /// </summary>
    public static Mystcraft<Unit> WriteMap(GameId Game, List<MapLevel> Map) =>
        new Mystcraft<Unit>.WriteMap(Game, Map, Return);

    /// <summary>
    /// Change game ruleset.
    /// </summary>
    public static Mystcraft<Unit> WritRuleset(GameId Game, Stream Ruleset) =>
        new Mystcraft<Unit>.WritRuleset(Game, Ruleset, Return);

    /// <summary>
    /// Change game engine.
    /// </summary>
    public static Mystcraft<Unit> WritEngine(GameId Game, GameEngineId Engine) =>
        new Mystcraft<Unit>.WritEngine(Game, Engine, Return);

    /// <summary>
    /// Get a list of all player registrations.
    /// </summary>
    public static Mystcraft<IQueryable<DbRegistration>> ReadManyRegistrations(GameId Game, Expression<Func<DbRegistration, bool>> Predicate) =>
        new Mystcraft<IQueryable<DbRegistration>>.ReadManyRegistrations(Game, Predicate, Return);

    /// <summary>
    /// Get a single player registration.
    /// </summary>
    public static Mystcraft<DbRegistration> ReadOneRegistration(RegistrationId Registration) =>
        new Mystcraft<DbRegistration>.ReadOneRegistration(Registration, Return);

    /// <summary>
    /// Register a player into list of new factions to be added into the game in the next game turn.
    /// </summary>
    public static Mystcraft<DbRegistration> RegisterPlayer(GameId Game, string Name, string Password) =>
        new Mystcraft<DbRegistration>.RegisterPlayer(Game, Name, Password, Return);

    /// <summary>
    /// Remove a player registration from the game.
    /// </summary>
    public static Mystcraft<RegistrationId> RemoveRegistration(RegistrationId Registration) =>
        new Mystcraft<RegistrationId>.RemoveRegistration(Registration, Return);

    /// <summary>
    /// Get a list of all players.
    /// </summary>
    public static Mystcraft<IQueryable<DbPlayer>> ReadManyPlayers(GameId Game, bool IncludeQuit, Expression<Func<DbPlayer, bool>> Predicate) =>
        new Mystcraft<IQueryable<DbPlayer>>.ReadManyPlayers(Game, IncludeQuit, Predicate, Return);

    /// <summary>
    /// Get a single player.
    /// </summary>
    public static Mystcraft<DbPlayer> ReadOnePlayer(PlayerId Player) =>
        new Mystcraft<DbPlayer>.ReadOnePlayer(Player, Return);

    /// <summary>
    /// Quite a player faction from the game in the next game turn.
    /// </summary>
    public static Mystcraft<PlayerId> QuitPlayer(PlayerId Player) =>
        new Mystcraft<PlayerId>.QuitPlayer(Player, Return);


    public static Mystcraft<DbTurn> CreateTurn(GameId gameId, TurnNumber turnNumber) =>
        new Mystcraft<DbTurn>.CreateTurn(Return);

    public static Mystcraft<DbTurn> WriteOneTurn(GameId Game, TurnNumber Turn) =>
        new Mystcraft<DbTurn>.WriteOneTurn(Game, Turn, Return);

    public static Mystcraft<Seq<FactionOrders>> ReadTurnOrders(GameId Game, TurnNumber Turn) =>
        new Mystcraft<Seq<FactionOrders>>.ReadTurnOrders(Game, Turn, Return);

    /// <summary>
    /// Parse the turn report.
    /// </summary>
    public static Mystcraft<JReport> ParseReport(Stream Report) =>
        new Mystcraft<JReport>.ParseReport(Report, Return);


    public static Mystcraft<WorkFolder> OpenWorkFolder() =>
        new Mystcraft<WorkFolder>.OpenWorkFolder(Return);

    public static Mystcraft<WorkFolderWithInput> WriteGameState(WorkFolder Folder,  GameEngineStream Engine, PlayersInStream PlayersIn, GameInStream GameIn, Seq<FactionOrders> Orders) =>
        new Mystcraft<WorkFolderWithInput>.WriteGameState(Folder, Engine, PlayersIn, GameIn, Orders, Return);

    public static Mystcraft<GameRunResult> RunEngine(WorkFolderWithInput Folder, TimeSpan Timeout) =>
        new Mystcraft<GameRunResult>.RunEngine(Folder, Timeout, Return);

    public static Mystcraft<PlayersOutStream> ReadPlayers(WorkFolderWithOutput Folder) =>
        new Mystcraft<PlayersOutStream>.ReadPlayers(Folder, Return);

    public static Mystcraft<GameOutStream> ReadGame(WorkFolderWithOutput Folder) =>
        new Mystcraft<GameOutStream>.ReadGame(Folder, Return);

    public static Mystcraft<Seq<ReportStream>> ReadReports(WorkFolderWithOutput Folder) =>
        new Mystcraft<Seq<ReportStream>>.ReadReports(Folder, Return);

    public static Mystcraft<Seq<MessageStream>> ReadMessages(WorkFolderWithOutput Folder) =>
        new Mystcraft<Seq<MessageStream>>.ReadMessages(Folder, Return);

/*
    public sealed record OpenWorkFolder(Func<WorkFolder, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record WriteGameEngine(WorkFolder Folder, DbGameEngine Engine, Func<WorkFolderWithEngine, Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record WriteGameState(WorkFolderWithEngine Folder, Stream PlayersIn, Stream GameIn, Func<WorkFolderWithInput, Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record WriteOrders(WorkFolderWithInput Folder, FactionNumber Faction, Stream Orders, Func<WorkFolderWithInput, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record RunEngine(WorkFolderWithInput Folder, Func<GameRunResult, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record ReadPlayers(WorkFolderWithOutput Folder, Func<Stream, Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record ReadGame(WorkFolderWithOutput Folder, Func<Stream, Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record ReadReports(WorkFolderWithOutput Folder, Func<FileInfo[], Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record ReadOrders(WorkFolderWithOutput Folder, Func<FileInfo[], Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record ReadMessages(WorkFolderWithOutput Folder, Func<FileInfo[], Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record CloseWorkFolder(WorkFolder Folder, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;
*/
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
    ///// Game engine creation and retrieval

    /// <summary>
    /// Represents operation that creates a new game engine.
    /// </summary>
    public sealed record CreateGameEngine(string Name, string Description, byte[] Engine, byte[] Ruleset, Func<DbGameEngine, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that creates a new remote game engine.
    /// </summary>
    public sealed record CreateGameEngineRemote(string Name, string Description, string Api, string Url, string Options, Func<DbGameEngine, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a list of all game engines.
    /// </summary>
    public sealed record ReadManyGameEngines(Option<Expression<Func<DbGameEngine, bool>>> Predicate, Func<IOrderedQueryable<DbGameEngine>, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a single game engine in a read-only mode.
    /// </summary>
    public sealed record ReadOneGameEngine(GameEngineId Engine, Func<ReadOnly<DbGameEngine>, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a single game engine prepared for modification.
    /// </summary>
    public sealed record WriteOneGameEngine(GameEngineId Engine, Func<DbGameEngine, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that deletes a game engine.
    /// </summary>
    public sealed record DeleteGameEngine(DbGameEngine Engine, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;



    /////////////////////////////////////////////
    ///// Game creation and retrieval

    /// <summary>
    /// Create a new game.
    /// </summary>
    public sealed record CreateGameRemote(string Name, GameEngineId Engine, List<MapLevel> Levels, GameSchedule Schedule, GamePeriod Period, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record CreateGameLocal(string Name, GameEngineId Engine, List<MapLevel> Levels, GameSchedule Schedule, GamePeriod Period, byte[] GameIn, byte[] PlayersIn, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a list of all games.
    /// </summary>
    public sealed record ReadManyGames(Option<Expression<Func<DbGame, bool>>> Predicate, Func<IOrderedQueryable<DbGame>, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a single game in a read-only mode.
    /// </summary>
    public sealed record ReadOneGame(GameId Game, Func<ReadOnly<DbGame>, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that gets a single game prepared for modification.
    /// </summary>
    public sealed record WriteOneGame(GameId Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// Turn creation and retrieval

    public sealed record CreateTurn(Func<DbTurn, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record WriteOneTurn(GameId Game, TurnNumber Turn, Func<DbTurn, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record ReadTurnOrders(GameId Game, TurnNumber Turn, Func<Seq<FactionOrders>, Mystcraft<A>> Next) : Mystcraft<A>;


    /////////////////////////////////////////////
    ///// State transitions

    /// <summary>
    /// Represents operation that starts a game.
    /// </summary>
    public sealed record Start(DbGame Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that pauses a game.
    /// </summary>
    public sealed record Pause(DbGame Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that locks a game and prevents new registrations and order updates.
    /// </summary>
    public sealed record Lock(DbGame Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that fully stops a game.
    /// </summary>
    public sealed record Stop(DbGame Game, Func<DbGame, Mystcraft<A>> Next) : Mystcraft<A>;

    /// <summary>
    /// Represents operation that deletes a game.
    /// </summary>
    public sealed record DeleteGame(DbGame Game, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;


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
    public sealed record WritEngine(GameId Game, GameEngineId Engine, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;


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
    /// Represents operation that parses the turn report.
    /// </summary>
    public sealed record ParseReport(Stream Report, Func<JReport, Mystcraft<A>> Next) : Mystcraft<A>;

    // public sealed record ParseTurn(PlayerId Player, TurnNumber Turn, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    // public sealed record MergeTurn(PlayerId Player, TurnNumber Turn, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    // public sealed record CollectTurnStatistics(PlayerId Player, TurnNumber Turn, Func<Unit, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record OpenWorkFolder(Func<WorkFolder, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record WriteGameState(WorkFolder Folder,  GameEngineStream Engine, PlayersInStream PlayersIn, GameInStream GameIn, Seq<FactionOrders> Orders, Func<WorkFolderWithInput, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record RunEngine(WorkFolderWithInput Folder, TimeSpan Timeout, Func<GameRunResult, Mystcraft<A>> Next) : Mystcraft<A>;

    public sealed record ReadPlayers(WorkFolderWithOutput Folder, Func<PlayersOutStream, Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record ReadGame(WorkFolderWithOutput Folder, Func<GameOutStream, Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record ReadReports(WorkFolderWithOutput Folder, Func<Seq<ReportStream>, Mystcraft<A>> Next) : Mystcraft<A>;
    public sealed record ReadMessages(WorkFolderWithOutput Folder, Func<Seq<MessageStream>, Mystcraft<A>> Next) : Mystcraft<A>;
}

public record WorkFolder(string Value) : IDisposable {
    private bool disposed = false;

    public static WorkFolder New(string value) => new (value);

    public Eff<string> File(string name) =>
        Eff(() => System.IO.Path.Combine(Value, name));

    public void Dispose() {
        if (disposed) {
            return;
        }

        System.IO.Directory.Delete(Value, true);
    }
}

public sealed record WorkFolderWithInput : WorkFolder
{
    public WorkFolderWithInput(string value) : base(value)
    {
    }

    public static new WorkFolderWithInput New(WorkFolder folder) => new (folder.Value);
}

public sealed record WorkFolderWithOutput : WorkFolder
{
    public WorkFolderWithOutput(string value) : base(value)
    {
    }

    public static new WorkFolderWithOutput New(WorkFolder folder) => new (folder.Value);
}

public record struct GameRunResult(WorkFolderWithOutput WorkFolder, bool Success, int ExitCode, Option<string> StdOut, Option<string> StdErr) {
    public static GameRunResult New(WorkFolderWithOutput workFolder, bool success, int exitCode, Option<string> stdOut, Option<string> stdErr) => new (workFolder, success, exitCode, stdOut, stdErr);
}

public record struct ReportStream(Stream Stream, string FileName) {
    public static ReportStream New(Stream stream, string fileName) => new (stream, fileName);
}

public record struct OrdersStream(Stream Stream, string FileName) {
    public static OrdersStream New(Stream stream, string fileName) => new (stream, fileName);
}

public record struct MessageStream(Stream Stream, string FileName) {
    public static MessageStream New(Stream stream, string fileName) => new (stream, fileName);
}

public record struct GameInStream(Stream Stream) {
    public static GameInStream New(Stream stream) => new (stream);
    public static GameInStream New(byte[] buffer) => new (new MemoryStream(buffer, false));
}

public record struct GameOutStream(Stream Stream) {
    public static GameOutStream New(Stream stream) => new (stream);
    public static GameOutStream New(byte[] buffer) => new (new MemoryStream(buffer, false));
}

public record struct PlayersInStream(Stream Stream) {
    public static PlayersInStream New(Stream stream) => new (stream);
    public static PlayersInStream New(byte[] buffer) => new (new MemoryStream(buffer, false));
}

public record struct PlayersOutStream(Stream Stream) {
    public static PlayersOutStream New(Stream stream) => new (stream);
    public static PlayersOutStream New(byte[] buffer) => new (new MemoryStream(buffer, false));
}

public record struct GameEngineStream(Stream Stream) {
    public static GameEngineStream New(Stream stream) => new (stream);
    public static GameEngineStream New(byte[] buffer) => new (new MemoryStream(buffer, false));
}
