namespace atlantis {
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class ReportType : ObjectType<DbReport> {
        protected override void Configure(IObjectTypeDescriptor<DbReport> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Reports.FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }
}
