namespace advisor.Features {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;

    public record CalculateFactionStats(long PlayerId, int EarliestTurnNumber) : IRequest<DbGame>;

    public class CalculateFactionStatsHandler : IRequestHandler<CalculateFactionStats, DbGame> {
        public CalculateFactionStatsHandler(Database db)
        {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbGame> Handle(CalculateFactionStats request, CancellationToken cancellationToken) {
            var player = await db.Players.SingleOrDefaultAsync(x => x.Id == request.PlayerId);

            var data = await db.Events
                .AsNoTrackingWithIdentityResolution()
                .FilterByPlayer(player)
                .Where(x => x.TurnNumber >= request.EarliestTurnNumber && x.FactionNumber == player.Number && x.RegionId != null)
                .ToListAsync();


            await DeleteStatItems(request.PlayerId, request.EarliestTurnNumber);
            await DeleteStats(request.PlayerId, request.EarliestTurnNumber);

            foreach (var turn in data.GroupBy(x => x.TurnNumber)) {
                var turnNumber = turn.Key;

                foreach (var region in turn.GroupBy(x => x.RegionId)) {
                    var regionId = region.Key;
                    var regionStat = Reduce(region);

                    regionStat.PlayerId = request.PlayerId;
                    regionStat.TurnNumber = turnNumber;
                    regionStat.RegionId = regionId;

                    foreach (var p in regionStat.Production) {
                        p.PlayerId = request.PlayerId;
                        p.TurnNumber = turnNumber;
                        p.RegionId = regionId;
                    }

                    await db.Stats.AddAsync(regionStat);
                }
            }

            await db.SaveChangesAsync();

            return null;
        }

        private Task DeleteStatItems(long playerId, int turnNumber) {
            var entity = db.Model.FindEntityType(typeof(DbStatItem));

            var table = entity.GetTableName();
            var schema = entity.GetSchema();

            var playerIdColumn = entity.FindProperty(nameof(DbStatItem.PlayerId)).GetColumnName(StoreObjectIdentifier.Table(table, schema));
            var turnNumberColumn = entity.FindProperty(nameof(DbStatItem.TurnNumber)).GetColumnName(StoreObjectIdentifier.Table(table, schema));

            return db.Database.ExecuteSqlRawAsync($@"delete from {table} where {playerIdColumn} = {playerId} and {turnNumberColumn} >= {turnNumber}");
        }

        private Task DeleteStats(long playerId, int turnNumber) {
            var entity = db.Model.FindEntityType(typeof(DbStat));

            var table = entity.GetTableName();
            var schema = entity.GetSchema();

            var playerIdColumn = entity.FindProperty(nameof(DbStat.PlayerId)).GetColumnName(StoreObjectIdentifier.Table(table, schema));
            var turnNumberColumn = entity.FindProperty(nameof(DbStat.TurnNumber)).GetColumnName(StoreObjectIdentifier.Table(table, schema));

            return db.Database.ExecuteSqlRawAsync($@"delete from {table} where {playerIdColumn} = {playerId} and {turnNumberColumn} >= {turnNumber}");
        }

        public static void SyncProduction(ICollection<DbStatItem> production, IEnumerable<DbStatItem> update) {
            var dest = production
                .ToDictionary(x => x.Code);

            foreach (var item in update) {
                if (!dest.TryGetValue(item.Code, out var d)) {
                    production.Add(item);

                    continue;
                }

                d.Amount = item.Amount;
            }

            // missing items are removed
            foreach (var key in dest.Keys.Except(update.Select(x => x.Code))) {
                production.Remove(dest[key]);
            }
        }

        private DbStat Reduce(IEnumerable<DbEvent> events) {
            var income = new DbIncomeStats();
            var production = new List<DbStatItem>();

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
                            .Select(item => new DbStatItem {
                                Code = item.Key,
                                Amount = item.Sum(x => x.Amount ?? 1)
                            }));
                        break;
                }
            }

            return new DbStat {
                Income = income,
                Production = production
            };
        }
    }
}
