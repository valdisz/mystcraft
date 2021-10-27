namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;


    public class TurnType : ObjectType<DbTurn> {
        protected override void Configure(IObjectTypeDescriptor<DbTurn> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompsiteId)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    var parsedId = DbTurn.ParseId(id);
                    return DbTurn.FilterById(db.Turns.AsNoTracking(), parsedId).SingleOrDefaultAsync();
                });
        }
    }

    [ExtendObjectType("Turn")]
    public class TurnResolvers {
        public Task<List<DbReport>> GetReports(Database db, [Parent] DbTurn turn) {
            return db.Reports
                .AsNoTracking()
                .FilterByTurn(turn)
                .OrderBy(x => x.FactionNumber)
                .ToListAsync();
        }

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbRegion> GetRegions(Database db, [Parent] DbTurn turn, bool withStructures = false) {
            IQueryable<DbRegion> query = db.Regions
                .AsSplitQuery()
                .AsNoTrackingWithIdentityResolution()
                .FilterByTurn(turn)
                .Include(x => x.Exits)
                .Include(x => x.Produces)
                .Include(x => x.Markets);

            if (withStructures) {
                query = query.Include(x => x.Structures);
            }

            return query.OrderBy(x => x.Id);
        }

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbStructure> Structures(Database db, [Parent] DbTurn turn) {
            return db.Structures
                .AsNoTracking()
                .OrderBy(x => x.RegionId)
                .ThenBy(x => x.Sequence)
                .FilterByTurn(turn);
        }

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbUnit> Units(Database db, [Parent] DbTurn turn) {
            return db.Units
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Items)
                .OrderBy(x => x.Number)
                .FilterByTurn(turn);
        }

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbEvent> Events(Database db, [Parent] DbTurn turn) {
            return db.Events
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FilterByTurn(turn);
        }

        public Task<List<DbFaction>> Factions(Database db, [Parent] DbTurn turn) {
            return db.Factions
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Attitudes)
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
