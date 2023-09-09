namespace advisor;

using System;
using advisor.Model;
using advisor.Persistence;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using LanguageExt.Sys.IO;
using LanguageExt.Sys.Traits;
using LanguageExt.Pipes;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Live interpreter for the basic operations that can be performed on a game.
/// </summary>
public readonly struct GameInterpreter<RT>
    where RT : struct, HasDatabase<RT>, HasUnitOfWork<RT>, HasDirectory<RT>, HasFile<RT>
{
    /// <summary>
    /// Interpret the Game DSL as an asynchronous effect.
    /// Will apply all write operations in a transaction.
    /// If operation fails, it will rollback any started transactions.
    /// </summary>
    public static Aff<RT, A> Interpret<A>(Mystcraft<A> dsl) =>
        _interpret(dsl) | @catch(_rollback<A>);

    private static Aff<RT, A> _next<R, A>(Aff<RT, R> ret, Func<R, Mystcraft<A>> next) =>
        ret.Select(next).Bind(_interpret);

    private static Aff<RT, A> _interpret<A>(Mystcraft<A> dsl) => dsl switch
    {
        Mystcraft<A>.Return rt => SuccessAff(rt.Value),

        Mystcraft<A>.CreateGameEngine action       => _tran(_next(CreateGameEngine(action), action.Next)),
        Mystcraft<A>.CreateGameEngineRemote action => _tran(_next(CreateGameEngineRemote(action), action.Next)),
        Mystcraft<A>.ReadManyGameEngines action    =>       _next(ReadManyGameEngines(action), action.Next),
        Mystcraft<A>.ReadOneGameEngine action      =>       _next(ReadOneGameEngine(action), action.Next),
        Mystcraft<A>.WriteOneGameEngine action     => _tran(_next(WriteOneGameEngine(action), action.Next)),
        Mystcraft<A>.DeleteGameEngine action       =>       _next(DeleteGameEngine(action), action.Next),
        Mystcraft<A>.CreateGameRemote action       => _tran(_next(CreateGameRemote(action), action.Next)),
        Mystcraft<A>.CreateGameLocal action        => _tran(_next(CreateGameLocal(action), action.Next)),
        Mystcraft<A>.ReadManyGames action          =>       _next(ReadManyGames(action), action.Next),
        Mystcraft<A>.ReadOneGame action            =>       _next(ReadOneGame(action), action.Next),
        Mystcraft<A>.WriteOneGame action           => _tran(_next(WriteOneGame(action), action.Next)),
        Mystcraft<A>.Start action                  =>       _next(StartGame(action), action.Next),
        Mystcraft<A>.Pause action                  =>       _next(PauseGame(action), action.Next),
        Mystcraft<A>.Lock action                   =>       _next(LockGame(action), action.Next),
        Mystcraft<A>.Stop action                   =>       _next(StopGame(action), action.Next),
        Mystcraft<A>.DeleteGame action             =>       _next(DeleteGame(action), action.Next),
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
        Mystcraft<A>.OpenWorkFolder action         => _next(OpenWorkFolder(action), action.Next),
        Mystcraft<A>.WriteGameEngine action        => _next(WriteGameEngine(action), action.Next),
        Mystcraft<A>.WriteGameState action         => _next(WriteGameState(action), action.Next),
        Mystcraft<A>.WriteOrders action            => _next(WriteOrders(action), action.Next),
        Mystcraft<A>.RunEngine action              => _next(RunEngine(action), action.Next),
        Mystcraft<A>.ReadPlayers action            => _next(ReadPlayers(action), action.Next),
        Mystcraft<A>.ReadGame action               => _next(ReadGame(action), action.Next),
        Mystcraft<A>.ReadReports action            => _next(ReadReports(action), action.Next),
        Mystcraft<A>.ReadOrders action             => _next(ReadOrders(action), action.Next),
        Mystcraft<A>.ReadMessages action           => _next(ReadMessages(action), action.Next),
        Mystcraft<A>.CloseWorkFolder action        => _next(CloseWorkFolder(action), action.Next),

        _ => FailAff<A>(E_OPERATION_NOT_SUPPORTED)
    };

    private static Aff<RT, A> _rollback<A>(Error failure) =>
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

    private static Aff<RT, DbGameEngine> CreateGameEngine<A>(Mystcraft<A>.CreateGameEngine action) =>
        from ge in Database<RT>.add(DbGameEngine.NewLocal(
            action.Name,
            action.Description,
            action.Engine,
            action.Ruleset
        ))
        from _ in UnitOfWork<RT>.save()
        select ge;

    private static Aff<RT, DbGameEngine> CreateGameEngineRemote<A>(Mystcraft<A>.CreateGameEngineRemote action) =>
        from ge in Database<RT>.add(DbGameEngine.NewRemote(
            action.Name,
            action.Description,
            action.Api,
            action.Url,
            action.Options
        ))
        from _ in UnitOfWork<RT>.save()
        select ge;

    private static Aff<RT, DbGame> CreateGameRemote<A>(Mystcraft<A>.CreateGameRemote action) =>
        // TODO: improve this
        from game in Database<RT>.add(DbGame.NewRemote(
            action.Name,
            action.Engine.Value,
            new GameOptions
            {
                Map = action.Levels,
                Schedule = action.Schedule.Cron,
                TimeZone = action.Schedule.TimeZone,
                StartAt = action.Period.StartAt.ToNullable(),
                FinishAt = action.Period.FinishAt.ToNullable()
            }
        ))
        from _ in UnitOfWork<RT>.save()
        select game;

    private static Aff<RT, DbGame> CreateGameLocal<A>(Mystcraft<A>.CreateGameLocal action) =>
        // TODO: improve this
        from game in Database<RT>.add(DbGame.NewLocal(
            action.Name,
            action.Engine.Value,
            new GameOptions
            {
                Map = action.Levels,
                Schedule = action.Schedule.Cron,
                TimeZone = action.Schedule.TimeZone,
                StartAt = action.Period.StartAt.ToNullable(),
                FinishAt = action.Period.FinishAt.ToNullable()
            },
            action.GameIn,
            action.PlayersIn
        ))
        from _ in UnitOfWork<RT>.save()
        select game;

    private static Aff<RT, IOrderedQueryable<DbGameEngine>> ReadManyGameEngines<A>(Mystcraft<A>.ReadManyGameEngines action) =>
        from list in Database<RT>.GameEngines
        let selection = action.Predicate
            .Match(
                Some: p => list.Where(p),
                None: () => list
            )
            .OrderByDescending(g => g.CreatedAt)
        select selection;

    private static Aff<RT, ReadOnly<DbGameEngine>> ReadOneGameEngine<A>(Mystcraft<A>.ReadOneGameEngine action) =>
        from list in Database<RT>.GameEngines
        from item in list
            .AsNoTracking()
            .Where(g => g.Id == action.Engine.Value)
            .HeadOrFailAff<RT, DbGameEngine>(E_GAME_ENGINE_DOES_NOT_EXIST)
        select ReadOnly.New(item);

    private static Aff<RT, DbGameEngine> WriteOneGameEngine<A>(Mystcraft<A>.WriteOneGameEngine action) =>
        from list in Database<RT>.GameEngines
        from item in list
            .Where(g => g.Id == action.Engine.Value)
            .HeadOrFailAff<RT, DbGameEngine>(E_GAME_ENGINE_DOES_NOT_EXIST)
        select item;

    private static Aff<RT, Unit> DeleteGameEngine<A>(Mystcraft<A>.DeleteGameEngine action) =>
        from games in Database<RT>.Games
        from hasGames in Aff<RT, bool>(rt => games.AnyAsync(g => g.EngineId == action.Engine.Id, rt.CancellationToken).ToValue())
        from _1 in guard(!hasGames, E_GAME_ENGINE_IS_IN_USE)
        from _2 in Database<RT>.delete(action.Engine)
        from _3 in UnitOfWork<RT>.save()
        select unit;

    private static Aff<RT, IOrderedQueryable<DbGame>> ReadManyGames<A>(Mystcraft<A>.ReadManyGames action) =>
        from list in Database<RT>.Games
        let selection = action.Predicate
            .Match(
                Some: p => list.Where(p),
                None: () => list
            )
            .OrderByDescending(g => g.CreatedAt)
        select selection;

    private static Aff<RT, ReadOnly<DbGame>> ReadOneGame<A>(Mystcraft<A>.ReadOneGame action) =>
        from games in Database<RT>.Games
        from game in games
            .AsNoTracking()
            .Where(g => g.Id == action.Game.Value)
            .HeadOrFailAff<RT, DbGame>(E_GAME_DOES_NOT_EXIST)
        select ReadOnly.New(game);

    private static Aff<RT, DbGame> WriteOneGame<A>(Mystcraft<A>.WriteOneGame action) =>
        from games in Database<RT>.Games
        from game in games
            .Where(g => g.Id == action.Game.Value)
            .HeadOrFailAff<RT, DbGame>(E_GAME_DOES_NOT_EXIST)
        select game;

    private static Either<Error, DbGame> CanStartGame(DbGame game) => game switch {
        { Status: GameStatus.RUNNING } => Left(E_GAME_ALREADY_RUNNING),
        _                              => Right(game)
    };

    private static Aff<RT, DbGame> StartGame<A>(Mystcraft<A>.Start action) =>
        from game in CanStartGame(action.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.RUNNING)
        select game;

    private static Either<Error, DbGame> CanPauseGame(DbGame game) => game switch {
        { Status: GameStatus.NEW }    => Left(E_GAME_MUST_BE_RUNNING),
        { Status: GameStatus.PAUSED } => Left(E_GAME_ALREADY_PAUSED),
        { Status: GameStatus.LOCKED } => Left(E_GAME_LOCKED),
        { Status: GameStatus.STOPED } => Left(E_GAME_STOPED),
        _                             => Right(game)
    };

    private static Aff<RT, DbGame> PauseGame<A>(Mystcraft<A>.Pause action) =>
        from game in CanPauseGame(action.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.RUNNING)
        select game;

    private static Either<Error, DbGame> CanLockGame(DbGame game) => game switch {
        { Status: GameStatus.NEW }    => Left(E_GAME_MUST_BE_RUNNING),
        { Status: GameStatus.PAUSED } => Left(E_GAME_PAUSED),
        { Status: GameStatus.LOCKED } => Left(E_GAME_ALREADY_LOCKED),
        { Status: GameStatus.STOPED } => Left(E_GAME_STOPED),
        _                             => Right(game)
    };

    private static Aff<RT, DbGame> LockGame<A>(Mystcraft<A>.Lock action) =>
        from game in CanLockGame(action.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.LOCKED)
        select game;

    private static Either<Error, DbGame> CanStopGame(DbGame game) => game switch {
        { Status: GameStatus.STOPED } => Left(E_GAME_ALREADY_STOPED),
        _                             => Right(game)
    };

    private static Aff<RT, DbGame> StopGame<A>(Mystcraft<A>.Stop action) =>
        from game in CanStopGame(action.Game).ToEff()
        from _ in _modify(game, g => g.Status = GameStatus.STOPED)
        select game;

    private static Aff<RT, Unit> DeleteGame<A>(Mystcraft<A>.DeleteGame action) =>
        from games in Database<RT>.Games
        from _1 in Eff<RT, EntityEntry<DbGame>>(x => games.Remove(action.Game))
        from _2 in UnitOfWork<RT>.save()
        select unit;

    private static Aff<RT, WorkFolder> OpenWorkFolder<A>(Mystcraft<A>.OpenWorkFolder action) =>
        from dirName in Eff(() => {
            // TODO: create runtime for IO.Path
            var tempPath = System.IO.Path.GetTempPath();
            var tempName = System.IO.Path.GetRandomFileName();
            var tempDir = System.IO.Path.Combine(tempPath, tempName);
            return tempDir;
        })
        from _ in Directory<RT>.create(dirName)
        select WorkFolder.New(dirName);

    private static Eff<string> workFolderFile(WorkFolder folder, string name) =>
        Eff(() => System.IO.Path.Combine(folder.Value, name));

    private static Producer<RT, System.IO.Stream, Unit> openWriteFile(string fileName) =>
        File<RT>.openWrite(fileName);

    private static Eff<RT, System.IO.Stream> openReadFile(string fileName) =>
        from file in default(RT).FileEff
        select file.OpenRead(fileName);

    private static Eff<string> gameEngineFile(WorkFolder folder) =>
        workFolderFile(folder, "engine");

    private static Eff<string> gameInFile(WorkFolderWithEngine folder) =>
        workFolderFile(folder, "game.in");

    private static Eff<string> playersInFile(WorkFolderWithEngine folder) =>
        workFolderFile(folder, "players.in");

    private static Eff<string> ordersFile(WorkFolderWithInput folder, FactionNumber num) =>
        workFolderFile(folder, $"orders.{num.Value}");

    private static Eff<string> gameOutFile(WorkFolderWithOutput folder) =>
        workFolderFile(folder, "game.out");

    private static Eff<string> playersOutFile(WorkFolderWithOutput folder) =>
        workFolderFile(folder, "players.out");

    private static Consumer<RT, System.IO.Stream, Unit> writeFromStream(System.IO.Stream stream) =>
        from dest in awaiting<System.IO.Stream>()
        from _ in Aff<RT, Unit>(async rt => {
            await stream.CopyToAsync(dest, rt.CancellationToken);
            return unit;
        })
        select unit;

    private static Aff<RT, WorkFolderWithEngine> WriteGameEngine<A>(Mystcraft<A>.WriteGameEngine action) =>
        from fileName in gameEngineFile(action.Folder)
        from _1 in (openWriteFile(fileName) | writeFromStream(action.Engine.Stream)).RunEffectUnit()
        from _2 in Eff(() => {
            _ = UnixInterop.chmod(
                fileName,
                UnixInterop.S_IXUSR | UnixInterop.S_IRUSR | UnixInterop.S_IWUSR |
                UnixInterop.S_IXGRP | UnixInterop.S_IRGRP |
                UnixInterop.S_IXOTH | UnixInterop.S_IROTH
            );
            return unit;
        })
        select WorkFolderWithEngine.New(action.Folder);

    private static Aff<RT, WorkFolderWithInput> WriteGameState<A>(Mystcraft<A>.WriteGameState action) =>
        from gameFile in gameInFile(action.Folder)
        from playersFile in playersInFile(action.Folder)
        from _1 in (openWriteFile(gameFile) | writeFromStream(action.GameIn.Stream)).RunEffectUnit()
        from _2 in (openWriteFile(playersFile) | writeFromStream(action.PlayersIn.Stream)).RunEffectUnit()
        select WorkFolderWithInput.New(action.Folder);


    private static Aff<RT, Unit> WriteOrders<A>(Mystcraft<A>.WriteOrders action) =>
        from ordersFile in ordersFile(action.Folder, action.Faction)
        from _ in (openWriteFile(ordersFile) | writeFromStream(action.Orders.Stream)).RunEffectUnit()
        select unit;

    private static Aff<RT, GameRunResult> RunEngine<A>(Mystcraft<A>.RunEngine action) =>
        from gameEngine in gameEngineFile(action.Folder)
        from ret in Aff<RT, GameRunResult>(async rt => {
            using var p = new Process();
            p.StartInfo = new ProcessStartInfo {
                WorkingDirectory = action.Folder.Value,
                FileName = gameEngine,
                Arguments = "run",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var cts = CancellationTokenSource.CreateLinkedTokenSource(rt.CancellationToken);
            cts.CancelAfter(action.Timeout);

            try {
                p.Start();
                await p.WaitForExitAsync(cts.Token);
            }
            catch (TaskCanceledException) {
                p.Kill(true);

                throw;
            }

            var exitCode = p.ExitCode;
            var success = exitCode == 0;
            var stdout = await p.StandardOutput.ReadToEndAsync();
            var stderr = await p.StandardError.ReadToEndAsync();

            return GameRunResult.New(
                WorkFolderWithOutput.New(action.Folder),
                success,
                exitCode,
                string.IsNullOrWhiteSpace(stdout) ? None : Some(stdout),
                string.IsNullOrWhiteSpace(stderr) ? None : Some(stderr)
            );
        })
        select ret;

    private static Aff<RT, PlayersOutStream> ReadPlayers<A>(Mystcraft<A>.ReadPlayers action) =>
        from playersFile in playersOutFile(action.Folder)
        from stream in openReadFile(playersFile)
        select PlayersOutStream.New(stream);

    private static Aff<RT, GameOutStream> ReadGame<A>(Mystcraft<A>.ReadGame action) =>
        from gameFile in gameOutFile(action.Folder)
        from stream in openReadFile(gameFile)
        select GameOutStream.New(stream);

    private static readonly Regex REPORT_PATTERN = new Regex(@"report\.\d+");

    private static Aff<RT, Seq<ReportStream>> ReadReports<A>(Mystcraft<A>.ReadReports action) =>
        from file in default(RT).FileEff
        from files in Directory<RT>.enumerateFiles(action.Folder.Value)
        let reportFiles = files.Filter(f => REPORT_PATTERN.IsMatch(f))
        select reportFiles.Select(f => ReportStream.New(file.OpenRead(f), f));

    private static readonly Regex ORDERS_PATTERN = new Regex(@"template\.\d+");

    private static Aff<RT, Seq<OrdersStream>> ReadOrders<A>(Mystcraft<A>.ReadOrders action) =>
        from fa in default(RT).FileEff
        from files in Directory<RT>.enumerateFiles(action.Folder.Value)
            .Select(seq => seq.Filter(ORDERS_PATTERN.IsMatch))
        let orderFiles = files
            .Select(f => OrdersStream.New(fa.OpenRead(f), f))
        select orderFiles;

    private static readonly Regex MESSAGE_PATTERN = new Regex(@"times\.\d+");

    private static Aff<RT, Seq<MessageStream>> ReadMessages<A>(Mystcraft<A>.ReadMessages action) =>
        from fa in default(RT).FileEff
        from files in Directory<RT>.enumerateFiles(action.Folder.Value)
            .Select(seq => seq.Filter(MESSAGE_PATTERN.IsMatch))
        let messageFiles = files
            .Select(f => MessageStream.New(fa.OpenRead(f), f))
        select messageFiles;

    private static Aff<RT, Unit> CloseWorkFolder<A>(Mystcraft<A>.CloseWorkFolder action) =>
        from _ in Directory<RT>.delete(action.Folder.Value)
        select unit;
}
