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
        public UnitResolvers(Database db, IIdSerializer idSerializer) {
            this.db = db;
            this.idSerializer = idSerializer;
        }

        private readonly Database db;
        private readonly IIdSerializer idSerializer;

        public string RegionId([Parent] DbUnit unit) {
            return idSerializer.Serialize("Region", unit.RegionId);
        }

        public string StructureId([Parent] DbUnit unit) {
            return unit.StrcutureId.HasValue
                ? idSerializer.Serialize("Structure", unit.StrcutureId)
                : null;
        }

        public Task<DbRegion> Region([Parent] DbUnit unit) {
            return db.Regions
                .SingleOrDefaultAsync(x => x.Id == unit.RegionId);
        }
    }
}
