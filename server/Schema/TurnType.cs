namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class TurnType : ObjectType<DbTurn> {
        protected override void Configure(IObjectTypeDescriptor<DbTurn> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Turns.SingleOrDefaultAsync(x => x.Id == id);
                });
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
                .Where(x => x.TurnId == turn.Id)
                .ToListAsync();
        }

        [UsePaging]
        public IQueryable<DbRegion> GetRegions([Parent] DbTurn turn) {
            return db.Regions
                .Where(x => x.TurnId == turn.Id);
        }

        [UsePaging]
        public IQueryable<DbStructure> GetStructures([Parent] DbTurn turn) {
            return db.Structures
                .Include(x => x.Region)
                .Where(x => x.TurnId == turn.Id);
        }

        [UsePaging]
        public IQueryable<DbUnit> GetUnits([Parent] DbTurn turn) {
            return db.Units
                .Include(x => x.Faction)
                .Where(x => x.TurnId == turn.Id);
        }
        public Task<List<DbFaction>> GetFactions([Parent] DbTurn turn) {
            return db.Factions
                .Where(x => x.TurnId == turn.Id)
                .ToListAsync();
        }

        public Task<List<DbEvent>> GetEvents([Parent] DbTurn turn) {
            return db.Events
                .Include(x => x.Faction)
                .Where(x => x.TurnId == turn.Id)
                .ToListAsync();
        }

        public Task<DbUnit> UnitByNumber([Parent] DbTurn turn, int number) {
            return db.Units
                .Include(x => x.Faction)
                .SingleOrDefaultAsync(x => x.TurnId == turn.Id && x.Number == number);
        }

        public Task<DbRegion> RegionByCoords([Parent] DbTurn turn, int x, int y, int z) {
            return db.Regions
                .SingleOrDefaultAsync(r => r.TurnId == turn.Id && r.X == x && r.Y == y && r.Z == z);
        }
    }
}
