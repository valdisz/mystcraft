namespace advisor {
    using System.Linq;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    using ReportId = System.ValueTuple<long, int, int>;

    public class ReportType : ObjectType<DbReport> {
        protected override void Configure(IObjectTypeDescriptor<DbReport> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompsiteId)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return DbReport.FilterById(db.Reports.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }
    }
}
