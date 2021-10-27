namespace advisor {
    using System.Linq;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;


    public class StructureType : ObjectType<DbStructure> {
        protected override void Configure(IObjectTypeDescriptor<DbStructure> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompositeId)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return DbStructure.FilterById(db.Structures.AsNoTracking(), id).SingleOrDefaultAsync();
                });

            descriptor.Field("code").Resolve(x => x.Parent<DbStructure>().Id);

            descriptor.Field("regionCode").Resolve(x => x.Parent<DbStructure>().RegionId);
        }
    }

    [ExtendObjectType("Structure")]
    public class StructureResolvers {
    }
}
