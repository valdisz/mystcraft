namespace advisor {
    using System.Linq;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    using ReportId = System.ValueTuple<long, int, int>;

    public class ReportType : ObjectType<DbReport> {
        protected override void Configure(IObjectTypeDescriptor<DbReport> descriptor) {
            descriptor.AsNode()
                .IdField(x => MakeId(x))
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return FilterById(db.Reports.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }

        private static ReportId MakeId(DbReport report) => (report.PlayerId, report.TurnNumber, report.FactionNumber);

        private static IQueryable<DbReport> FilterById(IQueryable<DbReport> q, ReportId id) {
            var (playerId, turnNumber, factionNumber) = id;
            return q.Where(x =>
                    x.PlayerId == playerId
                && x.TurnNumber == turnNumber
                && x.FactionNumber == factionNumber
            );
        }
    }
}
