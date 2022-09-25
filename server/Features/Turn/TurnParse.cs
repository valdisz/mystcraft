namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text;
using advisor.TurnProcessing;
using System;
using System.Collections.Generic;

public record TurnParse(DbGame Game, int TurnNumber, bool Force = false, long[] PlayerIds = null): IRequest<TurnParseResult>;

public record TurnParseResult(bool IsSuccess, string Error = null, DbTurn Turn = null) : MutationResult(IsSuccess, Error);

public class TurnParseHandler : IRequestHandler<TurnParse, TurnParseResult> {
    public TurnParseHandler(IUnitOfWork unit, IReportParser parser) {
        this.unit = unit;
        this.parser = parser;
    }

    private readonly IUnitOfWork unit;
    private readonly IReportParser parser;

    public async Task<TurnParseResult> Handle(TurnParse request, CancellationToken cancellationToken) {
        var turnsRepo = unit.Turns(request.Game);

        var query = turnsRepo.GetReports(request.TurnNumber);
        if (!request.Force) {
            query = query.Where(x => x.Json == null);
        }

        if ((request?.PlayerIds?.Length ?? 0) > 0) {
            query = query.Where(x => request.PlayerIds.Contains(x.PlayerId));
        }

        await foreach (var report in query.AsAsyncEnumerable().WithCancellation(cancellationToken)) {
            try {
                using var ms = new MemoryStream(report.Data);
                using var reader = new StreamReader(ms);
                var jsonString = await parser.ToJsonStringAsync(reader);

                report.Json = Encoding.UTF8.GetBytes(jsonString);
                report.Error = null;
            }
            catch (Exception ex) {
                report.Error = ex.ToString();
            }
        }

        await unit.SaveChangesAsync(cancellationToken);

        var turn = await turnsRepo.GetOneNoTrackingAsync(request.TurnNumber);

        return new TurnParseResult(true, Turn: turn);
    }
}
