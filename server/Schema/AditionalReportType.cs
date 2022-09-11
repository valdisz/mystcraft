namespace advisor.Schema {
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class AditionalReportType : ObjectType<DbAditionalReport> {
        protected override void Configure(IObjectTypeDescriptor<DbAditionalReport> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompsiteId)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return DbAditionalReport.FilterById(db.AditionalReports.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }
    }
}
