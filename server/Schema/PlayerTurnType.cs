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
                .ResolveNode((ctx, id) => {
                    var (playerId, turnNumber) = DbPlayerTurn.ParseId(id);
                    var db = ctx.Service<Database>();
                    return db.PlayerTurns
                        .AsNoTracking()
                        .Include(x => x.Player)
                        .SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber);
                });
        }
    }

    [ExtendObjectType("PlayerTurn")]
    public class PlayerTurnResolvers {
        public Task<List<DbAditionalReport>> GetReports(Database db, [Parent] DbPlayerTurn turn) {
            return db.AditionalReports
                .AsNoTracking()
                .InTurn(turn)
                .OrderBy(x => x.FactionNumber)
                .ToListAsync();
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

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbRegion> GetRegions(Database db, [Parent] DbPlayerTurn turn, bool withStructures = false) {
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

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbStructure> Structures(Database db, [Parent] DbPlayerTurn turn) {
            return db.Structures
                .AsNoTracking()
                .InTurn(turn)
                .OrderBy(x => x.RegionId)
                .ThenBy(x => x.Sequence);
        }

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public async Task<IQueryable<DbUnit>> Units(IResolverContext context, Database db, [Parent] DbPlayerTurn turn, UnitsFilter filter = null) {
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

            if (filter != null) {
                if (filter.Own != null) {
                    var factionNumber = turn.Player.Number;

                    query = filter.Own.Value
                        ? query.Where(x => x.FactionNumber == factionNumber)
                        : query.Where(x => x.FactionNumber != factionNumber);
                }

                if (filter.Mages != null) {
                    query = (await query.ToListAsync())
                        .Where(x => x.Skills.Any(s => s.Code == "FORC" || s.Code == "PATT" || s.Code == "SPIR"))
                        .AsQueryable();
                }
            }

            return query.OrderBy(x => x.Number);
        }

        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbEvent> Events(Database db, [Parent] DbPlayerTurn turn) {
            return db.Events
                .AsNoTracking()
                .InTurn(turn)
                .OrderBy(x => x.Id);
        }

        public Task<List<DbFaction>> Factions(Database db, [Parent] DbPlayerTurn turn) {
            return db.Factions
                .AsNoTrackingWithIdentityResolution()
                .Include(x => x.Attitudes)
                .OrderBy(x => x.Number)
                .InTurn(turn)
                .ToListAsync();
        }

        public async Task<Statistics> Stats(Database db, [Parent] DbPlayerTurn turn) {
            var factionNumber = turn.Player.Number;
            var stats = await db.Statistics
                .AsNoTracking()
                .InTurn(turn)
                // FIXME
                // .Include(x => x.Produced)
                .ToListAsync();

            DbIncome income = new DbIncome();
            Dictionary<string, int> production = new Dictionary<string, int>();

            foreach (var stat in stats) {
                income.Pillage += stat.Income.Pillage;
                income.Tax += stat.Income.Tax;
                income.Trade += stat.Income.Trade;
                income.Work += stat.Income.Work;

                // FIXME
                // foreach (var item in stat.Produced) {
                //     production[item.Code] = production.TryGetValue(item.Code, out var value)
                //         ? value + item.Amount
                //         : item.Amount;
                // }
            }

            return new Statistics {
                Income = income,
                Production = production.Select(x => new Item { Code = x.Key, Amount = x.Value }).ToList()
            };
        }

        public Task<List<DbStudyPlan>> StudyPlans(Database db, [Parent] DbPlayerTurn turn) {
            return db.StudyPlans
                .AsNoTrackingWithIdentityResolution()
                .InTurn(turn)
                .ToListAsync();
        }
    }
}
