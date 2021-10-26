namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public record FactionId(long Player, int Turn, int Faction);

    public class FactionType : ObjectType<DbFaction> {
        protected override void Configure(IObjectTypeDescriptor<DbFaction> descriptor) {
            descriptor.AsNode()
                .IdField(x => new FactionId(x.PlayerId, x.TurnNumber, x.Number))
                .NodeResolver((ctx, factionId) => {
                    var db = ctx.Service<Database>();
                    return db.Factions
                        .AsNoTracking()
                        .SingleOrDefaultAsync(x =>
                               x.PlayerId == factionId.Player
                            && x.TurnNumber == factionId.Turn
                            && x.Number == factionId.Faction
                        );
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
                .AsNoTracking()
                .FilterByFaction(faction)
                .ToListAsync();
        }

        public async Task<FactionStats> Stats([Parent] DbFaction faction) {
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
