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

    public class UniversityType : ObjectType<DbUniversity> {
        protected override void Configure(IObjectTypeDescriptor<DbUniversity> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Universities
                        .SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    public class TurnStats {
        public int Turn { get; set; }
        public List<FactionStats> Factions { get; set; }
    }

    [ExtendObjectType(Name = "University")]
    public class UniversityResolvers {
        public UniversityResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbUniversityMembership>> Members([Parent] DbUniversity university) {
            return db.UniversityMemberships
                .Include(x => x.Player)
                .Where(x => x.UniversityId == university.Id)
                .ToListAsync();
        }

        public async Task<List<UniversityClass>> Classes([Parent] DbUniversity university) {
            var turns = await db.StudyPlans
                .Include(x => x.Turn)
                .Where(x => x.UniversityId == university.Id)
                .Select(x => x.Turn.Number)
                .Distinct()
                .ToListAsync();

            return turns
                .Select(x => new UniversityClass(university.Id, x))
                .ToList();
        }

        private class StatProjection {
            public DbFaction Faction { get; set; }
            public int Turn { get; set; }
            public DbStat Stat { get; set; }
        }

        public async Task<List<TurnStats>> Stats([Parent] DbUniversity university) {
            var members = await db.UniversityMemberships
                .Include(x => x.Player)
                .Where(x => x.UniversityId == university.Id)
                .Select(x => new { x.Player.FactionNumber, x.PlayerId })
                .ToListAsync();

            var playerIds = members.Select(x => x.PlayerId).ToArray();

            var stats = await (from p in db.Players
                join t in db.Turns on p.Id equals t.PlayerId
                join f in db.Factions on t.Id equals f.TurnId
                join s in db.Stats on f.Id equals s.FactionId
                where playerIds.Contains(p.Id) && f.Number == p.FactionNumber
                select new StatProjection {
                    Faction = f,
                    Turn = t.Number,
                    Stat = s
                }).ToListAsync();


            var result = stats
                .GroupBy(x => x.Turn)
                .Select(turn => new TurnStats {
                    Turn = turn.Key,
                    Factions = turn
                        .GroupBy(x => (x.Faction.Number, x.Faction.Name))
                        .Select(faction => new FactionStats {
                            FactionName = faction.Key.Name,
                            FactionNumber = faction.Key.Number,
                            Income = faction
                                .Select(f => f.Stat.Income)
                                .Aggregate(new DbIncomeStats(), (value, next) => {
                                    value.Pillage += next.Pillage;
                                    value.Tax     += next.Tax;
                                    value.Trade   += next.Trade;
                                    value.Work    += next.Work;

                                    return value;
                                }),
                            Production = faction
                                .Select(f => f.Stat.Production)
                                .Aggregate(new Dictionary<string, int>(), (value, next) => {
                                    foreach (var item in next) {
                                        int amount = item.Amount ?? 0;
                                        value[item.Code] = value.TryGetValue(item.Code, out var v)
                                            ? v + amount
                                            : amount;
                                    }

                                    return value;
                                })
                                .Select(x => new DbItem { Code = x.Key, Amount = x.Value })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            return result;
        }
    }
}
