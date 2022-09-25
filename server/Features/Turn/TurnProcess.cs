namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using advisor.Model;

public record TurnProcess(DbGame Game, int TurnNumber, bool Force = false, long[] PlayerIds = null): IRequest<TurnProcessResult>;

public record TurnProcessResult(bool IsSuccess, string Error = null, DbTurn Turn = null) : MutationResult(IsSuccess, Error);

public class TurnProcessHandler : IRequestHandler<TurnProcess, TurnProcessResult> {
    public TurnProcessHandler(IUnitOfWork unit) {
        this.unit = unit;
        this.db = unit.Database;
    }

    private readonly IUnitOfWork unit;
    private readonly Database db;

    private record MapReduceItem();

    private record Statistics();

    public async Task<TurnProcessResult> Handle(TurnProcess request, CancellationToken cancellationToken) {
        var turnsRepo = unit.Turns(request.Game);
        var turn = await turnsRepo.GetOneNoTrackingAsync(request.TurnNumber, cancellationToken);
        if (turn == null) {
            return new TurnProcessResult(false, "Turn not found.");
        }

        await unit.BeginTransactionAsync(cancellationToken);

        await CalculateStatisticsAsync(turn.GameId, turn.Number);

        await unit.SaveChangesAsync(cancellationToken);
        await unit.CommitTransactionAsync(cancellationToken);

        return new TurnProcessResult(true, Turn: turn);
    }

    private async Task CalculateStatisticsAsync(long gameId, int turnNumber) {
        var playerTurns = db.PlayerTurns
            .InGame(gameId)
            .Where(x => !x.IsProcessed && x.TurnNumber == turnNumber)
            .AsAsyncEnumerable();

        await foreach (var pt in playerTurns) {
            var stats = await CalculatePlayerStatisticsAsync(pt.PlayerId, pt.TurnNumber);
            await db.Statistics.AddRangeAsync(stats);
            pt.IsProcessed = true;
        }
    }

    private async Task<IEnumerable<DbStatistics>> CalculatePlayerStatisticsAsync(long playerId, int turnNumber) {
        Dictionary<string, DbStatistics> items = new ();

        DbStatistics get(DbEvent ev) {
            var regionId = ev.RegionId;

            if (!items.TryGetValue(regionId, out var stats)) {
                stats = new DbStatistics {
                    PlayerId = playerId,
                    TurnNumber = turnNumber,
                    RegionId = regionId
                };

                items.Add(regionId, stats);
            }

            return stats;
        }

        DbStatisticsItem getItem(DbEvent ev, StatisticsCategory category) {
            var itemCode = ev.ItemCode;
            var list = get(ev).Items;

            var item = list.Find(x => x.Code == itemCode && x.Category == category);
            if (item == null) {
                item = new DbStatisticsItem {
                    PlayerId = playerId,
                    RegionId = ev.RegionId,
                    Code = itemCode,
                    TurnNumber = turnNumber
                };
                list.Add(item);
            }

            return item;
        }

        DbIncome income(DbEvent ev) => get(ev).Income;
        DbExpenses expenses(DbEvent ev) => get(ev).Expenses;
        DbStatisticsItem produced(DbEvent ev) => getItem(ev, StatisticsCategory.Produced);
        DbStatisticsItem bought(DbEvent ev) => getItem(ev, StatisticsCategory.Bought);
        DbStatisticsItem sold(DbEvent ev) => getItem(ev, StatisticsCategory.Sold);
        DbStatisticsItem consumed(DbEvent ev) => getItem(ev, StatisticsCategory.Consumed);

        var events = db.Events
            .AsNoTracking()
            .Where(x => x.PlayerId == playerId && x.TurnNumber == turnNumber)
            .AsAsyncEnumerable();

        await foreach (var ev in events) {
            if (ev.RegionId == null) {
                continue;
            }

            var amount = ev.Amount ?? 0;
            var price = ev.ItemPrice ?? 0;

            switch (ev.Category) {
                case EventCategory.Pillage:
                    income(ev).Pillage += amount;
                    break;

                case EventCategory.Tax:
                    income(ev).Tax += amount;
                    break;

                case EventCategory.Entertain:
                    income(ev).Entertain += amount;
                    break;

                case EventCategory.Work:
                    income(ev).Work += amount;
                    break;

                case EventCategory.Sell:
                    income(ev).Trade += amount * price;
                    sold(ev).Amount += amount;
                    break;

                case EventCategory.Buy:
                    expenses(ev).Trade += amount * price;
                    bought(ev).Amount += amount;
                    break;

                case EventCategory.Produce:
                    produced(ev).Amount += amount;
                    break;

                case EventCategory.Claim:
                    income(ev).Claim += amount;
                    break;

                case EventCategory.Cast:
                    income(ev).Entertain += amount;
                    break;

                case EventCategory.Consume:
                    if (ev.ItemCode == null) {
                        expenses(ev).Consume += amount;
                    }
                    else {
                        consumed(ev).Amount += amount;
                    }
                    break;

                case EventCategory.Withdraw:
                    income(ev).Entertain += amount;
                    break;

                case EventCategory.Give:
                case EventCategory.Study:
                case EventCategory.Unknown:
                    // do nothing
                    break;
            }
        }

        return items.Values;
    }
}
