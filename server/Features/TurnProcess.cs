namespace advisor.Features
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public record TurnProcess(long PlayerId, int EarliestTurn) : IRequest;

    public class TurnProcessHandler : IRequestHandler<TurnProcess> {
        public TurnProcessHandler(IMediator mediator, ILogger<TurnProcessHandler> logger) {
            this.mediator = mediator;
            this.logger = logger;
        }

        private readonly IMediator mediator;
        private readonly ILogger logger;

        public async Task<Unit> Handle(TurnProcess request, CancellationToken cancellationToken) {
            logger.LogInformation($"Parsing report");
            await mediator.Send(new ReportParse(request.PlayerId, request.EarliestTurn));

            logger.LogInformation($"Setting up study plans");
            await mediator.Send(new StudyPlansSetup(request.PlayerId));

            logger.LogInformation($"Calculating statistics");
            await mediator.Send(new CalculateFactionStats(request.PlayerId, request.EarliestTurn));

            return Unit.Value;
        }
    }

    public record TurnReProcess(long GameId, int Turn) : IRequest<TurnReProcessResult>;

    public record TurnReProcessResult(bool IsSuccess, string Error) : IMutationResult;

    public class TurnReProcessHandler : IRequestHandler<TurnReProcess, TurnReProcessResult> {
        public TurnReProcessHandler(Database db, IMediator mediator, ILogger<TurnProcessHandler> logger) {
            this.db = db;
            this.mediator = mediator;
            this.logger = logger;
        }

        private readonly Database db;
        private readonly IMediator mediator;
        private readonly ILogger logger;

        public async Task<TurnReProcessResult> Handle(TurnReProcess request, CancellationToken cancellationToken) {
            var players = await db.Players
                .Where(x => x.GameId == request.GameId)
                .Select(x => x.Id)
                .ToListAsync();

            foreach (var player in players) {
                await mediator.Send(new TurnProcess(player, request.Turn));
            }

            return new TurnReProcessResult(true, null);
        }
    }
}
