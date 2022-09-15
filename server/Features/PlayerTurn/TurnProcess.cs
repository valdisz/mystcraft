namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
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
        await mediator.Send(new PlayerReportParse(request.PlayerId, request.EarliestTurn));

        logger.LogInformation($"Setting up study plans");
        await mediator.Send(new StudyPlansSetup(request.PlayerId));

        logger.LogInformation($"Calculating statistics");
        await mediator.Send(new PlayerStatsCalculate(request.PlayerId, request.EarliestTurn));

        return Unit.Value;
    }
}
