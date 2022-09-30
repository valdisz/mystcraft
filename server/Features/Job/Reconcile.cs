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

public record Reconcile(long? GameId = null): IRequest<ReconcileResult>;
public record ReconcileResult(bool IsSuccess, string Error = null) : IMutationResult;

public class ReconcileHandler : IRequestHandler<Reconcile, ReconcileResult> {
    public ReconcileHandler(IUnitOfWork unit, IRecurringJobManager recurringJobs, IBackgroundJobClient backgroundJobs) {
        this.unit = unit;
        this.recurringJobs = recurringJobs;
        this.backgroundJobs = backgroundJobs;
    }

    private readonly IUnitOfWork unit;
    private readonly IRecurringJobManager recurringJobs;
    private readonly IBackgroundJobClient backgroundJobs;

    private record GameProjection(long Id, GameStatus Status, Persistence.GameType Type, GameOptions Options);

    public async Task<ReconcileResult> Handle(Reconcile request, CancellationToken cancellation) {
        var gamesRepo = unit.Games;

        var query = gamesRepo.Games
            .AsNoTracking();

        if (request.GameId != null) {
            query = query.Where(x => x.Id == request.GameId);
        }

        var projection = query.Select(x => new GameProjection(x.Id, x.Status, x.Type, x.Options));

        await foreach (var game in projection.AsAsyncEnumerable().WithCancellation(cancellation)) {
            ReconcileSingleGameJob(game);
        }

        if (request.GameId == null) {
            recurringJobs.AddOrUpdate<ReconcileJob>("reconcile", job => job.RunAsync(), "*/15 * * * *");
        }

        return new ReconcileResult(true);
    }

    private void ReconcileSingleGameJob(GameProjection game) {
        var jobIdPrefix = $"game-{game.Id}";

        var shouldRun = game.Status == GameStatus.RUNNING && !string.IsNullOrWhiteSpace(game.Options.Schedule);

        var nextTurnJobId = $"{jobIdPrefix}-turn";
        if (shouldRun) {
            var timeZone = FindTimeZone(game.Options.TimeZone);
            recurringJobs.AddOrUpdate<GameNextTurnJob>(nextTurnJobId, job => job.RunAsync(game.Id, null, null), game.Options.Schedule, timeZone);
        }
        else {
            recurringJobs.RemoveIfExists(nextTurnJobId);
        }

        if (game.Type == Persistence.GameType.REMOTE) {
            var factionSyncJobId = $"{jobIdPrefix}-factions";

            if (shouldRun) {
                recurringJobs.AddOrUpdate<GameSyncFactionsJob>(factionSyncJobId, job => job.RunAsync(game.Id), "*/5 * * * *");
            }
            else {
                recurringJobs.RemoveIfExists(factionSyncJobId);
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

