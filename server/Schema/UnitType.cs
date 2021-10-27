namespace advisor {
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UnitType : ObjectType<DbUnit> {
        protected override void Configure(IObjectTypeDescriptor<DbUnit> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompositeId)
                .ResolveNode((ctx, id) => {
                    var parsedId = DbUnit.ParseId(id);
                    var db = ctx.Service<Database>();
                    return DbUnit.FilterById(db.Units.AsNoTracking(), parsedId).SingleOrDefaultAsync();
                });

            descriptor.Field("regionCode").Resolve(x => x.Parent<DbUnit>().RegionId);

            descriptor.Field("structureNumber").Resolve(x => x.Parent<DbUnit>().StrcutureNumber);
        }
    }

    [ExtendObjectType("Unit")]
    public class UnitResolvers {
    }
}
