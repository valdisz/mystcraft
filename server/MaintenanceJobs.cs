namespace advisor
{
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using Hangfire.RecurringJobExtensions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class MaintenanceJobs {
        public MaintenanceJobs(Database db, IMediator mediator, ILogger<MaintenanceJobs> logger) {
            this.db = db;
            this.mediator = mediator;
            this.logger = logger;
        }

        private readonly Database db;
        private readonly IMediator mediator;
        private readonly ILogger logger;

        // every hour
        [RecurringJob("0 * * * *", TimeZone = "America/Los_Angeles")]
        public async Task ReconcileRports() {
            logger.LogInformation($"Looking for unparsed reports");

            var reportsWithoutJson = await db.Reports
                .Include(x => x.Turn)
                .ThenInclude(x => x.Player)
                .Where(x => x.Json == null)
                .ToListAsync();

            logger.LogInformation($"Found {reportsWithoutJson.Count} unparsed reports");
        }
    }
}
