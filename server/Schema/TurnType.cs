namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    using TurnId = System.ValueTuple<long, int>;

    public class TurnType : ObjectType<DbTurn> {
        protected override void Configure(IObjectTypeDescriptor<DbTurn> descriptor) {
            descriptor.AsNode()
                .IdField(x => MakeId(x))
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return FilterById(db.Turns.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }

        public static TurnId MakeId(long playerId, int turnNumber) => (playerId, turnNumber);
        public static TurnId MakeId(DbTurn turn) => (turn.PlayerId, turn.Number);

        private static IQueryable<DbTurn> FilterById(IQueryable<DbTurn> q, TurnId id) {
            var (playerId, turnNumber) = id;
            return q.Where(x => x.PlayerId == playerId && x.Number == turnNumber);
        }
    }

    [ExtendObjectType(Name = "Turn")]
    public class TurnResolvers {
        public TurnResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbReport>> GetReports([Parent] DbTurn turn) {
            return db.Reports
                .AsNoTracking()
                .FilterByTurn(turn)
                .OrderBy(x => x.FactionNumber)
                .ToListAsync();
        }

        [UsePaging]
        public IQueryable<DbRegion> GetRegions([Parent] DbTurn turn) {
            return db.Regions
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FilterByTurn(turn);
        }

        [UsePaging]
        public IQueryable<DbStructure> Structures([Parent] DbTurn turn) {
            return db.Structures
                .AsNoTracking()
                .OrderBy(x => x.RegionId)
                .ThenBy(x => x.Sequence)
                .FilterByTurn(turn);
        }

        [UsePaging]
        public IQueryable<DbUnit> Units([Parent] DbTurn turn) {
            return db.Units
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Items)
                .OrderBy(x => x.Number)
                .FilterByTurn(turn);
        }

        public Task<List<DbFaction>> Factions([Parent] DbTurn turn) {
            return db.Factions
                .AsNoTracking()
                .OrderBy(x => x.Number)
                .FilterByTurn(turn)
                .ToListAsync();
        }

        // public Task<List<DbEvent>> GetEvents([Parent] DbTurn turn) {
        //     return db.Events
        //         .Include(x => x.Faction)
        //         .Where(x => x.TurnId == turn.Id)
        //         .ToListAsync();
        // }

        // public Task<DbUnit> UnitByNumber([Parent] DbTurn turn, int number) {
        //     return db.Units
        //         .Include(x => x.Faction)
        //         .SingleOrDefaultAsync(x => x.TurnId == turn.Id && x.Number == number);
        // }

        // public Task<DbRegion> RegionByCoords([Parent] DbTurn turn, int x, int y, int z) {
        //     return db.Regions
        //         .SingleOrDefaultAsync(r => r.TurnId == turn.Id && r.X == x && r.Y == y && r.Z == z);
        // }
    }
}
