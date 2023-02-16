namespace advisor.Features;

using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System;
using Microsoft.Extensions.Logging;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

public delegate AsyncIO<T> StagePipeline<T>(IServiceScope scope, CancellationToken cancellation);

public class AllJobs {
    public AllJobs(IGameRepository gameRepo, IMediator mediator, IServiceProvider services, IBackgroundJobClient jobs, ILogger<AllJobs> logger) {
        this.gameRepo = gameRepo;
        this.mediator = mediator;
        this.services = services;
        this.jobs = jobs;
        this.logger = logger;
    }

    private readonly IGameRepository gameRepo;
    private readonly IMediator mediator;
    private readonly IServiceProvider services;
    private readonly IBackgroundJobClient jobs;
    private readonly ILogger logger;

    public Task ReconcileAsync(CancellationToken cancellation)
        => mediator.Send(new Reconcile(), cancellation);

    public Task SyncFactionsAsync(long gameId, CancellationToken cancellation)
        => mediator.Send(new GameSyncFactions(gameId), cancellation);

    public record ProcessingState(long gameId, int turnNumber, TurnState state);

    public async Task RunTurnAsync(long gameId, GameNextTurnForceInput force, CancellationToken cancellation) {
        await Log(LogLevel.Information, "Starting turn processing")
            .Bind(() => PrepareProcessing(gameId, cancellation))
            .Bind(x => StageWithMediator("Execute engine",
                condition:     x.state == TurnState.PENDING,
                pipeline:      (m, _) => m.Mutate(new TurnRun(x.gameId, x.turnNumber, 0, null), cancellation)
                                    .Select(ret => x with { state = ret.Turn.State }),
                defaultResult: x,
                cancellation:  cancellation
            ))
            .Bind(x => StageWithMediator("Parse reports",
                condition:     x.state == TurnState.EXECUTED,
                pipeline:      (m, _) => m.Mutate(new TurnParse(x.gameId, x.turnNumber, Force: force?.Parse ?? false), cancellation)
                                    .Select(ret => x with { state = ret.Turn.State }),
                defaultResult: x,
                cancellation:  cancellation
            ))
            .Bind(x => StageWithMediator("Merge reports",
                condition:    x.state == TurnState.PARSED,
                input:        new TurnMerge(gameId, turn.Number, Force: force?.Merge ?? false),
                cancellation: cancellation
            ))
            .Bind(x => StageWithMediator("Statistics",
                condition:    x.state == TurnState.MERGED,
                input:        new TurnProcess(gameId, turn.Number, Force: force?.Process ?? false),
                cancellation: cancellation
            ))
            .Bind(x => Stage("Finish",
                    condition:    x.state == TurnState.PROCESSED,
                    input:        FinshTurnProcessing(turn.GameId, turn.Number),
                    cancellation: cancellation
                )
                .Bind(() => Log(LogLevel.Information, $"Turn {x.} is ready"))
            )
            // todo: unlock game, set next turn, etc.
            .OnFailure(err => Log(LogLevel.Warning, err.Message))
            .Run();
    }

    public AsyncIO<ProcessingState> PrepareProcessing(long gameId, CancellationToken cancellation)
        => gameRepo.GetOneGame(gameId, withTracking: false, cancellation: cancellation)
            .Validate(game => game switch {
                { LastTurnNumber: null } => Failure<DbGame>("Last Turn is not specified"),
                { NextTurnNumber: null } => Failure<DbGame>("Next Turn is not specified"),
                { Status: GameStatus.LOCKED } => Success(game),
                { Status: GameStatus.RUNNING } => Success(game),
                _ => Failure<DbGame>("Game must be in RUNNING or LOCKED state")
            })
            .Select(game => ( gameId: game.Id, lastTurn: game.LastTurnNumber.Value, nextTurn: game.NextTurnNumber.Value ))
            .Bind(state => PickTurnToProcess(gameRepo.Specialize(state.gameId), state.lastTurn, state.nextTurn))
            .Select(turn => new ProcessingState(turn.GameId, turn.Number, turn.State));

    public AsyncIO<DbTurn> PickTurnToProcess(ISpecializedGameRepository repo, int lastTurnNumber, int nextTurnNumber)
        => repo.GetOneTurn(lastTurnNumber)
            .Bind(Functions.EnsurePresent<DbTurn>("Last Turn not found"))
            .Bind(lastTurn => lastTurn.State != TurnState.READY
                ? Log(LogLevel.Information, $"Will try to finish running turn {lastTurnNumber}")
                    .Return(lastTurn)
                    .AsAsync()
                : repo.GetOneTurn(nextTurnNumber)
                    .Bind(Functions.EnsurePresent<DbTurn>("Next Turn not found"))
                    .Bind(turn => Log(LogLevel.Information, $"Running turn {nextTurnNumber}")
                        .Return(turn)
                    )
            )
            .Bind(turn => turn switch {
                { State: TurnState.READY } => Failure<DbTurn>("Turn can't be in READY state"),
                _ => Success(turn)
            });

    public StagePipeline<advisor.Unit> FinshTurnProcessing(long gameId, int turnNumber)
        => (scope, cancellation) => RequiredService<IGameRepository>(scope)
            .Bind(gameRepo => gameRepo.GetOneGame(gameId, cancellation: cancellation)
                .Bind(Functions.EnsurePresent<DbGame>("Game not found"))
                // change game state
                .Do(game => game.Status = GameStatus.RUNNING)
                // set new turns
                .Bind(gameRepo.Update)
                .Bind(game => Effect(gameRepo.Specialize(game))
                    .Bind(repo => repo.GetOneTurn(turnNumber)
                        .Bind(Functions.EnsurePresent<DbTurn>("Turn not found"))
                        .Do(turn => turn.State = TurnState.READY)
                        .Bind(repo.Update)
                    )
                )
            )
            .Return(unit);

    private IO<T> RequiredService<T>(IServiceScope scope)
        => Effect(() => scope.ServiceProvider.GetRequiredService<T>());

    private IO<Tuple<T1, T2>> RequiredServices<T1, T2>(IServiceScope scope)
        => Effect(() => Tuple.Create(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>()
        ));

    private IO<Tuple<T1, T2, T3>> RequiredServices<T1, T2, T3>(IServiceScope scope)
        => Effect(() => Tuple.Create(
            scope.ServiceProvider.GetRequiredService<T1>(),
            scope.ServiceProvider.GetRequiredService<T2>(),
            scope.ServiceProvider.GetRequiredService<T3>()
        ));

    private IO<advisor.Unit> Log(LogLevel level, string message)
        => Effect(() => logger.Log(level, message));

    // we want to run each turn in a transaction and new scope
    private AsyncIO<T> Stage<T>(string name, bool condition, Func<IServiceScope, AsyncIO<T>> pipeline, T defaultResult, CancellationToken cancellation)
        => condition
            ? Effect(() => services.CreateScope())
                .Bind(scope => RequiredService<IUnitOfWork>(scope)
                    .Bind(unitOfWork => unitOfWork.BeginTransaction(cancellation)
                        .Bind(() => Log(LogLevel.Information, $"[Starting] {name}"))
                        .Bind(() => pipeline(scope))
                        .Bind(result => unitOfWork.CommitTransaction(cancellation).Return(result))
                        .OnFailure(err => unitOfWork.RollbackTransaction(cancellation))
                    )
                    .Finally(() => scope.Dispose())
                )
            : AsyncEffect(() => Log(LogLevel.Information, $"[Skipping] {name}").Run())
                .Return(defaultResult);

    private AsyncIO<T> StageWithMediator<T>(string name, bool condition, Func<IMediator, IServiceScope, AsyncIO<T>> pipeline, T defaultResult, CancellationToken cancellation)
        => Stage<T>(name, condition,
            scope => RequiredService<IMediator>(scope)
                .Bind(med => pipeline(med, scope)),
            defaultResult,
            cancellation
        );
}
