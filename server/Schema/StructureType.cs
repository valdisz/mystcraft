namespace advisor.Schema {
    using HotChocolate.Types;
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
        }
    }

    [ExtendObjectType("Structure")]
    public class StructureResolvers {
    }
}
