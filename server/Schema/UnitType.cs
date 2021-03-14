namespace advisor
{
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UnitType : ObjectType<DbUnit> {
        protected override void Configure(IObjectTypeDescriptor<DbUnit> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Units
                        .Include(x => x.Faction)
                        .SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Unit")]
    public class UnitResolvers {
        public UnitResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<DbRegion> Region([Parent] DbUnit unit) {
            return db.Regions
                .SingleOrDefaultAsync(x => x.Id == unit.RegionId);
        }
    }
}
