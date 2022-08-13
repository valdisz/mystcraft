namespace advisor.Features {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using Hangfire;
    using MediatR;

    public record GameOptionsSet(long GameId, GameOptions Options) : IRequest<DbGame>;

    public class GameOptionsSetHandler : IRequestHandler<GameOptionsSet, DbGame> {
        public GameOptionsSetHandler(Database db, IRecurringJobManager jobs) {
            this.db = db;
            this.jobs = jobs;
        }

        private readonly Database db;
        private readonly IRecurringJobManager jobs;

        public async Task<DbGame> Handle(GameOptionsSet request, CancellationToken cancellationToken) {
            var game = await db.Games.FindAsync(request.GameId);
            if (game == null) return null;

            game.Options = request.Options;
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

            return game;
        }
    }
}
