namespace advisor {
    using System.Linq;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    using RegionId = System.ValueTuple<long, int, string>;

    public class RegionType : ObjectType<DbRegion> {
        protected override void Configure(IObjectTypeDescriptor<DbRegion> descriptor) {
            descriptor.AsNode()
                .IdField(x => MakeId(x))
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return FilterById(db.Regions.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }

        public static RegionId MakeId(long playerId, int turnNumber, string regionId) => (playerId, turnNumber, regionId);
        public static RegionId MakeId(DbRegion region) => (region.PlayerId, region.TurnNumber, region.Id);

        public static IQueryable<DbRegion> FilterById(IQueryable<DbRegion> q, RegionId id) {
            var (playerId, turnNumber, regionId) = id;
            return q.Where(x =>
                    x.PlayerId == playerId
                && x.TurnNumber == turnNumber
                && x.Id == regionId
            );
        }
    }
}
