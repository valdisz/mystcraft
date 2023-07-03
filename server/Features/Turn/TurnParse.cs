// FIXME
namespace advisor.Features;

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using advisor.TurnProcessing;

public record TurnParse(long GameId, int TurnNumber, bool Force = false, long[] PlayerIds = null): IRequest<TurnParseResult>;

public record TurnParseResult(bool IsSuccess, string Error = null, DbTurn Turn = null) : MutationResult(IsSuccess, Error);

// public class TurnParseHandler : IRequestHandler<TurnParse, TurnParseResult> {
//     public TurnParseHandler(IUnitOfWork unit, IReportParser parser) {
//         this.unit = unit;
//         this.parser = parser;
//     }

//     private readonly IUnitOfWork unit;
//     private readonly IReportParser parser;

//     public async Task<TurnParseResult> Handle(TurnParse request, CancellationToken cancellationToken) {
//         var turnsRepo = unit.Turns(request.Game);

//         var reportQuery = turnsRepo.GetReports(request.TurnNumber);
//         var addReportQuery = unit.Database.AditionalReports
//             .Include(x => x.Player)
//             .Where(x => x.TurnNumber == request.TurnNumber && x.Player.GameId == request.Game.Id);


//         if (!request.Force) {
//             reportQuery = reportQuery.Where(x => x.Json == null);
//             addReportQuery = addReportQuery.Where(x => x.Json == null);
//         }

//         if ((request?.PlayerIds?.Length ?? 0) > 0) {
//             reportQuery = reportQuery.Where(x => request.PlayerIds.Contains(x.PlayerId));
//             addReportQuery = addReportQuery.Where(x => request.PlayerIds.Contains(x.PlayerId));
//         }

//         await foreach (var report in reportQuery.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
//             try {
//                 using var ms = new MemoryStream(report.Source);
//                 using var reader = new StreamReader(ms);
//                 var jsonString = await parser.ToJsonStringAsync(reader);

//                 report.Json = Encoding.UTF8.GetBytes(jsonString);
//                 report.Error = null;
//             }
//             catch (Exception ex) {
//                 report.Error = ex.ToString();
//             }
//         }

//         await foreach (var report in addReportQuery.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
//             try {
//                 using var ms = new MemoryStream(report.Source);
//                 using var reader = new StreamReader(ms);
//                 var jsonString = await parser.ToJsonStringAsync(reader);

//                 report.Json = Encoding.UTF8.GetBytes(jsonString);
//                 report.Error = null;
//             }
//             catch (Exception ex) {
//                 report.Error = ex.ToString();
//             }
//         }

//         await unit.SaveChanges(cancellationToken);

//         var turn = await turnsRepo.GetOneNoTrackingAsync(request.TurnNumber);

//         return new TurnParseResult(true, Turn: turn);
//     }
// }
