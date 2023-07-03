// FIXME
namespace advisor.Features;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public record TurnReProcess(long GameId, int Turn) : IRequest<TurnReProcessResult>;

public record TurnReProcessResult(bool IsSuccess, string Error) : IMutationResult;

// public class TurnReProcessHandler : IRequestHandler<TurnReProcess, TurnReProcessResult> {
//     public TurnReProcessHandler(Database db, ILogger<TurnProcessHandler> logger, IBackgroundJobClient background) {
//         this.db = db;
//         this.logger = logger;
//         this.background = background;
//     }

//     private readonly Database db;
//     private readonly ILogger logger;
//     private readonly IBackgroundJobClient background;

//     public async Task<TurnReProcessResult> Handle(TurnReProcess request, CancellationToken cancellationToken) {
//         var players = await db.Players
//             .Where(x => x.GameId == request.GameId && !x.IsQuit)
//             .Select(x => x.Id)
//             .ToListAsync();

//         foreach (var player in players) {
//             background.Enqueue<TurnReProcessJob>(x => x.Execute(new TurnProcess(player, request.Turn)));
//         }

//         return new TurnReProcessResult(true, null);
//     }
// }

// public class TurnReProcessJob {
//     public TurnReProcessJob(IMediator mediator) {
//         this.mediator = mediator;
//     }

//     private readonly IMediator mediator;

//     public Task Execute(TurnProcess task) => mediator.Send(task);
//     }
