namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using Hangfire;
using System;

public record GameScheduleSet(long GameId, string Schedule): IRequest<GameScheduleSetResult>;

public record GameScheduleSetResult(DbGame Game, bool IsSuccess, string Error) : IMutationResult;

public class GameScheduleSetHandler : IRequestHandler<GameScheduleSet, GameScheduleSetResult> {
    public GameScheduleSetHandler(Database db, IRecurringJobManager jobs) {
        this.db = db;
        this.jobs = jobs;
    }

    private readonly Database db;
    private readonly IRecurringJobManager jobs;

    public async Task<GameScheduleSetResult> Handle(GameScheduleSet request, CancellationToken cancellationToken) {
        var game = await db.Games.FindAsync(request.GameId);
        if (game == null) {
            return new GameScheduleSetResult(null, false, "Game not found");
        }

        game.Options.Schedule = request.Schedule;

        await db.SaveChangesAsync();

        var jobId = $"game-{game.Id}";
        if (string.IsNullOrWhiteSpace(game.Options.Schedule)) {
            jobs.RemoveIfExists(jobId);
        }
        else {
            if (game.Type == GameType.Remote) {
                jobs.AddOrUpdate<RemoteGameServerJobs>(
                    jobId,
                    x => x.NewOrigins(game.Id),
                    game.Options.Schedule,
                    TimeZoneInfo.FindSystemTimeZoneById(game.Options.TimeZone ?? "America/Los_Angeles")
                );
            }
        }

        return new GameScheduleSetResult(game, true, null);
    }
}
