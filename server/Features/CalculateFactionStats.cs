namespace advisor.Features {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record CalculateFactionStats(long PlayerId, int EarliestTurnNumber) : IRequest<DbGame> {

    }

    public class CalculateFactionStatsHandler : IRequestHandler<CalculateFactionStats, DbGame> {
        public CalculateFactionStatsHandler(Database db)
        {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(CalculateFactionStats request, CancellationToken cancellationToken) {
            var data = await db.Events
                .Include(x => x.Turn)
                .Where(x => x.Turn.PlayerId == request.PlayerId
                    && x.Turn.Number >= request.EarliestTurnNumber)
                .ToListAsync();

            foreach (var turn in data.GroupBy(x => x.Turn.Id)) {
                var turnId = turn.Key;

                foreach (var faction in turn.GroupBy(x => x.FactionId)) {
                    var factionId = faction.Key;

                    foreach (var region in faction.GroupBy(x => x.RegionId)) {
                        var regionId = region.Key;

                        var stat = await db.FactionStats
                            .SingleOrDefaultAsync(x => x.FactionId == factionId
                                && x.TurnId == turnId
                                && x.RegionId == regionId
                            );

                        var value = Reduce(region);

                        if (stat != null) {
                            stat.Income = value.Income;
                            stat.Production = value.Production;
                        }
                        else {
                            value.FactionId = factionId;
                            value.TurnId = turnId;
                            value.RegionId = regionId;

                            await db.FactionStats.AddAsync(value);
                        }
                    }
                }
            }

            await db.SaveChangesAsync();

            return null;
        }

        private DbFactionStats Reduce(IEnumerable<DbEvent> events) {
            var income = new DbIncomeStats();
            var production = new List<DbItem>();

            foreach (var category in events.GroupBy(x => x.Category)) {
                switch (category.Key) {
                    case Model.EventCategory.Pillage:
                        income.Pillage = category.Sum(x => x.Amount ?? 0);
                        break;

                    case Model.EventCategory.Work:
                        income.Work = category.Sum(x => x.Amount ?? 0);
                        break;

                    case Model.EventCategory.Tax:
                        income.Tax = category.Sum(x => x.Amount ?? 0);
                        break;

                    case Model.EventCategory.Sell:
                        income.Trade = category.Sum(x => (x.Amount ?? 0) * (x.ItemPrice ?? 0));
                        break;

                    case Model.EventCategory.Produce:
                        production.AddRange(category
                            .GroupBy(item => item.ItemCode)
                            .Select(item => new DbItem {
                                Code = item.Key,
                                Amount = item.Sum(x => x.Amount)
                            }));
                        break;
                }
            }

            return new DbFactionStats {
                Income = income,
                Production = production
            };
        }
    }
}
