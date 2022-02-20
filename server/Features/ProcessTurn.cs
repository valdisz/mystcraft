namespace advisor.Features
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public record ProcessTurn(long PlayerId, int EarliestTurn) : IRequest;

    public class ProcessTurnHandler : IRequestHandler<ProcessTurn> {
        public ProcessTurnHandler(IMediator mediator, ILogger<ProcessTurnHandler> logger) {
            this.mediator = mediator;
            this.logger = logger;
        }

        private readonly IMediator mediator;
        private readonly ILogger logger;

        public async Task<Unit> Handle(ProcessTurn request, CancellationToken cancellationToken) {
            logger.LogInformation($"Parsing report");
            await mediator.Send(new ParseReports(request.PlayerId, request.EarliestTurn));

            // logger.LogInformation($"Setting up study plans");
            // await mediator.Send(new SetupStudyPlans(request.PlayerId));

            logger.LogInformation($"Calculating statistics");
            await mediator.Send(new CalculateFactionStats(request.PlayerId, request.EarliestTurn));

            return Unit.Value;
        }
    }
}
