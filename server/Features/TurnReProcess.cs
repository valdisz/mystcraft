namespace advisor.Features
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

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
