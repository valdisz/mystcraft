namespace advisor.Features;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Hangfire;
using advisor.Persistence;
using advisor.Schema;
using System.Linq.Expressions;

public record Reconcile(long? GameId = null): IRequest<ReconcileResult>;
public record ReconcileResult(bool IsSuccess, string Error = null) : IMutationResult;

public class ReconcileHandler : IRequestHandler<Reconcile, ReconcileResult> {
    public ReconcileHandler(IGameRepository gameRepo, IRecurringJobManager recurringJobs, IBackgroundJobClient backgroundJobs) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.recurringJobs = recurringJobs;
        this.backgroundJobs = backgroundJobs;
    }

    public const string RECONCILE_ONCE_PER_HOUR = "0 0 * * *";
    public const string SYNC_FACTIONS_EVERY_5_MINUTES = "*/5 * * * *";

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IRecurringJobManager recurringJobs;
    private readonly IBackgroundJobClient backgroundJobs;

    private record GameProjection(long Id, GameStatus Status, Persistence.GameType Type, GameOptions Options);

    public async Task<ReconcileResult> Handle(Reconcile request, CancellationToken cancellation) {
        var projection = Some(gameRepo.Games.AsNoTracking())
            .Select(query => request.GameId.AsOption()
                .Select(gameId => query.Where(x => x.Id == gameId))
                .Unwrap(query)
                .Select(x => new GameProjection(x.Id, x.Status, x.Type, x.Options))
            )
            .Unwrap();

        await foreach (var game in projection.AsAsyncEnumerable().WithCancellation(cancellation)) {
            if (ReconcileSingleGame(game) is Result<advisor.Unit>.Failure(var err1)) {
                return new ReconcileResult(false, err1.Message);
            }
        }

        var setupReconcilation = Effect(() => recurringJobs.AddOrUpdate<AllJobs>("reconcile", job => job.ReconcileAsync(), RECONCILE_ONCE_PER_HOUR));
        if (setupReconcilation.Run() is Result<advisor.Unit>.Failure(var err2)) {
            return new ReconcileResult(false, err2.Message);
        }

        return new ReconcileResult(true);
    }

    private Result<advisor.Unit> ReconcileSingleGame(GameProjection game) {
        var jobIdPrefix = $"game-{game.Id}";

        var nextTurnJobId = $"{jobIdPrefix}-turn";
        var factionSyncJobId = $"{jobIdPrefix}-factions";

        return TimeZone(game.Options.TimeZone)
            .Bind(tz => DefineJob(
                $"{jobIdPrefix}-turn",
                game.Status.In(GameStatus.RUNNING, GameStatus.LOCKED) && !string.IsNullOrWhiteSpace(game.Options.Schedule),
                job => job.RunTurnAsync(game.Id, null, null),
                game.Options?.Schedule,
                tz
            ))
            .Bind(_ => DefineJob(
                $"{jobIdPrefix}-factions",
                game.Type == Persistence.GameType.REMOTE && game.Status == GameStatus.RUNNING,
                job => job.SyncFactionsAsync(game.Id),
                SYNC_FACTIONS_EVERY_5_MINUTES
            ))
            .Run();
    }

    private IO<TimeZoneInfo> TimeZone(string name)
        => Effect(() => TimeZoneInfo.FindSystemTimeZoneById(name))
            .OnFailure(_ => Success(TimeZoneInfo.Local));

    private IO<advisor.Unit> DefineJob(string id, bool condition, Expression<Func<AllJobs, Task>> setUp, string cronExpression, TimeZoneInfo tz = null)
        => Effect(() => {
            if (condition) {
                recurringJobs.AddOrUpdate<AllJobs>(id, setUp, cronExpression, tz);
            }
            else {
                recurringJobs.RemoveIfExists(id);
            }
        });
}

