namespace atlantis
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class RegionType : ObjectType<DbRegion> {
        protected override void Configure(IObjectTypeDescriptor<DbRegion> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Regions.FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Region")]
    public class RegionResolvers {
        public RegionResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbUnit>> GetUnits([Parent] DbRegion region) {
            return db.Units
                .Include(x => x.Faction)
                .Where(x => x.RegionId == region.Id)
                .ToListAsync();
        }

        public Task<List<DbStructure>> GetStructures([Parent] DbRegion region) {
            return db.Structures
                .Where(x => x.RegionId == region.Id)
                .ToListAsync();
        }

        public Task<DbUnit> UnitByNumber([Parent] DbRegion region, int number) {
            return db.Units
                .Include(x => x.Faction)
                .SingleOrDefaultAsync(x => x.RegionId == region.Id && x.Number == number);
        }

        public Task<DbStructure> StructureByNumber([Parent] DbRegion region, int number) {
            return db.Structures
                .SingleOrDefaultAsync(x => x.RegionId == region.Id && x.Number == number);
        }
    }
}
