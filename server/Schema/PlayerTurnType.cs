namespace advisor.Schema
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Model;
    using HotChocolate;
    using HotChocolate.Resolvers;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class PlayerTurnType : ObjectType<DbPlayerTurn> {
        protected override void Configure(IObjectTypeDescriptor<DbPlayerTurn> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode(async (ctx, id) => {
                    if (!await ctx.AuthorizeAsync(Policies.OwnPlayer)) {
                        return null;
                    }

                    var (playerId, turnNumber) = DbPlayerTurn.ParseId(id);
                    var db = ctx.Service<Database>();
                    return await db.PlayerTurns
                        .AsNoTracking()
                        // .Include(x => x.Player)
                        .SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber);
                });
        }
    }

    [ExtendObjectType("PlayerTurn")]
    public class PlayerTurnResolvers {
        [UseOffsetPaging]
        public IQueryable<DbAditionalReport> Reports(Database db, [Parent] DbPlayerTurn turn) {
            return db.AditionalReports
                .AsNoTracking()
                .InTurn(turn)
                .OrderBy(x => x.FactionNumber);
        }
        public async Task<List<JBattle>> Battles(Database db, [Parent] DbPlayerTurn turn) {
            var battles = (await db.Battles
                .AsNoTracking()
                .InTurn(turn)
                .OrderBy(x => x.X).ThenBy(x => x.Y).ThenBy(x => x.Z).ThenBy(x => x.Attacker.Number)
                .ToListAsync())
                .Select(x => x.Battle)
                .ToList();

            return battles;
        }

        [UseOffsetPaging(MaxPageSize = 1000)]
        public IQueryable<DbRegion> Regions(Database db, [Parent] DbPlayerTurn turn, bool withStructures = false) {
            IQueryable<DbRegion> query = db.Regions
                .AsSplitQuery()
                .AsNoTrackingWithIdentityResolution()
                .InTurn(turn)
                .Include(x => x.Exits)
                .Include(x => x.Produces)
                .Include(x => x.Markets);

            if (withStructures) {
                query = query.Include(x => x.Structures);
            }

            return query.OrderBy(x => x.Id);
        }

        [UseOffsetPaging(MaxPageSize = 1000)]
        public IQueryable<DbStructure> Structures(Database db, [Parent] DbPlayerTurn turn) {
            return db.Structures
                .AsNoTrackingWithIdentityResolution()
                .InTurn(turn)
                .OrderBy(x => x.RegionId)
                .ThenBy(x => x.Sequence);
        }

        [UseOffsetPaging(MaxPageSize = 1000)]
        public IQueryable<DbUnit> Units(IResolverContext context, Database db, [Parent] DbPlayerTurn turn, UnitsFilter filter = null) {
            var fields = context.CollectSelectedFields<DbUnit>();

            var query = db.Units
                .AsNoTrackingWithIdentityResolution()
                .InTurn(turn);

            if (fields.Contains(nameof(DbUnit.Items))) {
                query = query.Include(x => x.Items);
            }

            if (fields.Contains(nameof(DbUnit.StudyPlan))) {
                query = query.Include(x => x.StudyPlan);
            }

            if (filter?.Own != null) {
                var factionNumber = turn.FactionNumber;

                query = filter.Own.Value
                    ? query.Where(x => x.FactionNumber == factionNumber)
                    : query.Where(x => x.FactionNumber != factionNumber);
            }

            if (filter?.Mages != null) {
                query = query.Where(x => x.IsMage);
            }

            return query.OrderBy(x => x.Number);
        }

        [UseOffsetPaging(MaxPageSize = 1000)]
        public IQueryable<DbEvent> Events(Database db, [Parent] DbPlayerTurn turn) {
            return db.Events
                .AsNoTrackingWithIdentityResolution()
                .InTurn(turn)
                .OrderBy(x => x.Id);
        }

        public IQueryable<DbFaction> Factions(Database db, [Parent] DbPlayerTurn turn) {
            return db.Factions
                .AsNoTrackingWithIdentityResolution()
                .InTurn(turn);
        }

        public IQueryable<DbStudyPlan> StudyPlans(Database db, [Parent] DbPlayerTurn turn) {
            return db.StudyPlans
                .AsNoTrackingWithIdentityResolution()
                .InTurn(turn);
        }
    }
}
