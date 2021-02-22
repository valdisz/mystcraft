namespace atlantis {
    using System.Linq;
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

        [UsePaging]
        public IQueryable<DbUnit> GetUnits([Parent] DbRegion region) {
            return db.Units
                .Include(x => x.Faction)
                .Where(x => x.RegionId == region.Id);
        }
    }
}
