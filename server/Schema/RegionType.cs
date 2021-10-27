namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;


    public class RegionType : ObjectType<DbRegion> {
        protected override void Configure(IObjectTypeDescriptor<DbRegion> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompositeId)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return DbRegion.FilterById(db.Regions.AsNoTracking(), id).SingleOrDefaultAsync();
                });

            descriptor.Field("code").Resolve(x => x.Parent<DbRegion>().Id);
        }
    }

    [ExtendObjectType("Region")]
    public class RegionResolvers {
    }
}
