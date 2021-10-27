namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class FactionType : ObjectType<DbFaction> {
        protected override void Configure(IObjectTypeDescriptor<DbFaction> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompsiteId)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return DbFaction.FilterById(db.Factions.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }
    }

    [ExtendObjectType("Faction")]
    public class FactionResolvers {
        public Task<List<DbEvent>> Events(Database db, [Parent] DbFaction faction) {
            return db.Events
                .AsNoTracking()
                .FilterByFaction(faction)
                .ToListAsync();
        }

        public async Task<FactionStats> Stats(Database db, [Parent] DbFaction faction) {
            var stats = await db.Stats
                .AsNoTracking()
                .FilterByFaction(faction)
                .ToListAsync();

            DbIncomeStats income = new DbIncomeStats();
            Dictionary<string, int> production = new Dictionary<string, int>();

            foreach (var stat in stats) {
                income.Pillage += stat.Income.Pillage;
                income.Tax += stat.Income.Tax;
                income.Trade += stat.Income.Trade;
                income.Work += stat.Income.Work;

                foreach (var item in stat.Production) {
                    production[item.Code] = production.TryGetValue(item.Code, out var value)
                        ? value + item.Amount
                        : item.Amount;
                }
            }

            return new FactionStats {
                FactionNumber = faction.Number,
                FactionName = faction.Name,
                Income = income,
                Production = production.Select(x => new Item { Code = x.Key, Amount = x.Value }).ToList()
            };
        }
    }
}
