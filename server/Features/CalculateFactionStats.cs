namespace advisor.Features
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record CalculateFactionStats(long playerId, int EarliestTurnNumber) : IRequest<DbGame> {

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
                .Where(x => x.Turn.PlayerId == request.playerId
                    && x.Turn.Number >= request.EarliestTurnNumber)
                .ToListAsync();

            foreach (var turn in data.GroupBy(x => x.Turn.Id)) {
                var turnId = turn.Key;

                foreach (var faction in turn.GroupBy(x => x.FactionId)) {
                    var factionId = faction.Key;

                    var stat = await db.FactionStats.SingleOrDefaultAsync(x => x.FactionId == factionId && x.TurnId == turnId);
                    var newStat = CalculateFactionStats(faction);

                    if (stat != null) {
                        stat.Income = newStat.Income;
                        stat.Production = newStat.Production;
                    }
                    else {
                        newStat.FactionId = factionId;
                        newStat.TurnId = turnId;
                        await db.FactionStats.AddAsync(newStat);
                    }
                }
            }

            await db.SaveChangesAsync();

            return null;
        }

        private DbFactionStats CalculateFactionStats(IEnumerable<DbEvent> events) {
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
