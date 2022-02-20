namespace advisor {
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

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
