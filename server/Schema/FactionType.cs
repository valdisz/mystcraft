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

    public class FactionType : ObjectType<DbFaction> {
        protected override void Configure(IObjectTypeDescriptor<DbFaction> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Factions
                        .SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Faction")]
    public class FactionResolvers {
        public FactionResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbEvent>> GetEvents([Parent] DbFaction faction) {
            return db.Events
                .Include(x => x.Faction)
                .Where(x => x.FactionId == faction.Id)
                .ToListAsync();
        }

        public Task<DbUnit> UnitByNumber([Parent] DbFaction faction, int number) {
            return db.Units
                .Include(x => x.Faction)
                .SingleOrDefaultAsync(x => x.FactionId == faction.Id && x.Number == number);
        }

        public async Task<FactionStats> Stats([Parent] DbFaction faction) {
            var stats = await db.Stats
                .AsNoTracking()
                .AsSplitQuery()
                .Where(x => x.FactionId == faction.Id)
                .ToListAsync();

            DbIncomeStats income = new DbIncomeStats();
            Dictionary<string, int> production = new Dictionary<string, int>();

            foreach (var stat in stats) {
                income.Pillage += stat.Income.Pillage;
                income.Tax += stat.Income.Tax;
                income.Trade += stat.Income.Trade;
                income.Work += stat.Income.Work;

                foreach (var item in stat.Production) {
                    int amount = item.Amount ?? 0;
                    production[item.Code] = production.TryGetValue(item.Code, out var value)
                        ? value + amount
                        : amount;
                }
            }

            return new FactionStats {
                FactionNumber = faction.Number,
                FactionName = faction.Name,
                Income = income,
                Production = production.Select(x => new DbItem { Code = x.Key, Amount = x.Value }).ToList()
            };
        }
    }
}
