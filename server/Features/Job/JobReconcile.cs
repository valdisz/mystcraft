namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using System;

public record JobReconcile(long? GameId = null): IRequest<JobReconcileResult>;
public record JobReconcileResult(bool IsSuccess, string Error = null) : IMutationResult;

public class ReconcilationJob {
    public ReconcilationJob(IMediator mediator) {
        this.mediator = mediator;
    }

    private readonly IMediator mediator;

    public Task RunAsync() => mediator.Send(new JobReconcile());
}

public class JobReconcileHandler : IRequestHandler<JobReconcile, JobReconcileResult> {
    public JobReconcileHandler(IUnitOfWork unit, IRecurringJobManager jobs) {
        this.unit = unit;
        this.jobs = jobs;
    }

    private readonly IUnitOfWork unit;
    private readonly IRecurringJobManager jobs;

    private record GameProjection(long Id, GameStatus Status, Persistence.GameType Type, GameOptions Options);

    public async Task<JobReconcileResult> Handle(JobReconcile request, CancellationToken cancellation) {
        var gamesRepo = unit.Games;

        var query = gamesRepo.Games
            .AsNoTracking();

        if (request.GameId.HasValue) {
            query = query.Where(x => x.Id == request.GameId);
        }

        var projection = query.Select(x => new GameProjection(x.Id, x.Status, x.Type, x.Options));

        await foreach (var game in projection.AsAsyncEnumerable().WithCancellation(cancellation)) {
            ReconcileSingleGameJob(game, cancellation);
        }

        if (!request.GameId.HasValue) {
            jobs.AddOrUpdate<ReconcilationJob>("reconcile", job => job.RunAsync(), "*/5 * * * *");
        }

        return new JobReconcileResult(true);
    }

    private void ReconcileSingleGameJob(GameProjection game, CancellationToken cancellation) {
        var jobIdPrefix = $"game-{game.Id}";

        var shouldRun = game.Status == GameStatus.RUNNING && !string.IsNullOrWhiteSpace(game.Options.Schedule);
        var timeZone = FindTimeZone(game.Options.TimeZone);

        if (game.Type == Persistence.GameType.LOCAL) {
            // run turn job
        }
        else {
            var factionSyncJobId = $"{jobIdPrefix}-factions";

            // run remote turn job
            // sync faction status job

            if (shouldRun) {
                // jobs.AddOrUpdate<GameSyncFactionsJob>(factionSyncJobId, job => job.RunAsync(game.Id), "*/5 * * * *", timeZone);
            }
            else {
                jobs.RemoveIfExists(factionSyncJobId);
            }
        }
    }

    private TimeZoneInfo FindTimeZone(string name) {
        if (!string.IsNullOrWhiteSpace(null)) {
            try {
                return TimeZoneInfo.FindSystemTimeZoneById(name);
            }
            catch (InvalidTimeZoneException) {}
            catch  (TimeZoneNotFoundException) { }
        }

        return TimeZoneInfo.Local;
    }
}

