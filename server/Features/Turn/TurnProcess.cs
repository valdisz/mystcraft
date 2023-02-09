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
using System;

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

        await unit.BeginTransaction(cancellationToken);

        var q = db.PlayerTurns
            .InGame(turn.GameId)
            .Include(x => x.Player)
            .Include(x => x.Statistics)
            .Include(x => x.Treasury)
            .Where(x => x.Player.UserId != null && x.Player.Password != null && x.TurnNumber == request.TurnNumber);

        if (!request.Force) {
            q = q.Where(x => !x.IsProcessed);
        }

        await foreach (var pt in q.AsAsyncEnumerable()) {
            await CalculateStatisticsAsync(pt);
            await CalculateTreasuryAsync(pt);
            pt.IsProcessed = true;
        }

        await unit.CommitTransaction(cancellationToken);

        return new TurnProcessResult(true, Turn: turn);
    }
    private async Task CalculateTreasuryAsync(DbPlayerTurn turn) {
        var treasury = turn.Treasury.ToDictionary(x => x.Code);
        foreach (var item in treasury.Values) {
            item.Amount = 0;
        }

        var units = db.Units
            .Include(x => x.Items)
            .Where(x => x.PlayerId == turn.PlayerId && x.TurnNumber == turn.TurnNumber && x.FactionNumber == turn.FactionNumber)
            .AsAsyncEnumerable();

        await foreach (var unit in units) {
            foreach (var item in unit.Items) {
                if (!treasury.TryGetValue(item.Code, out var target)) {
                    target = new DbTreasuryItem {
                        PlayerId = turn.PlayerId,
                        TurnNumber = turn.TurnNumber,
                        Code = item.Code,
                        Amount = 0,
                    };

                    treasury.Add(target.Code, target);
                    await db.Treasury.AddAsync(target);
                }

                target.Amount += item.Amount;
            }
        }

        db.RemoveRange(treasury.Values.Where(x => x.Amount == 0));
    }

    private async Task CalculateStatisticsAsync(DbPlayerTurn turn) {
        var regions = await db.Regions
            .InTurn(turn)
            .Include(x => x.Statistics)
            .ToDictionaryAsync(x => x.Id);

        // Reset statistics
        foreach (var region in regions.Values) {
            region.Income.Work = 0;
            region.Income.Entertain = 0;
            region.Income.Tax = 0;
            region.Income.Pillage = 0;
            region.Income.Trade = 0;
            region.Income.Claim = 0;
            region.Expenses.Consume = 0;
            region.Expenses.Study = 0;
            region.Expenses.Trade = 0;

            foreach (var item in region.Statistics) {
                item.Amount = 0;
            }
        }

        turn.Income.Work = 0;
        turn.Income.Entertain = 0;
        turn.Income.Tax = 0;
        turn.Income.Pillage = 0;
        turn.Income.Trade = 0;
        turn.Income.Claim = 0;
        turn.Expenses.Consume = 0;
        turn.Expenses.Study = 0;
        turn.Expenses.Trade = 0;

        foreach (var item in turn.Statistics) {
            item.Amount = 0;
        }

        // Calculate new statistics
        DbRegion get(DbEvent ev) {
            var regionId = ev.RegionId;
            if (regionId == null) {
                return null;
            }

            if (!regions.TryGetValue(regionId, out var reg)) {
                return reg;
            }

            return null;
        }

        void income(DbEvent ev, Action<DbIncome> onIncome) {
            var income = get(ev)?.Income;
            if (income != null) {
                onIncome(income);
            }

            onIncome(turn.Income);
        }

        void expenses(DbEvent ev, Action<DbExpenses> onExpense) {
            var expenses = get(ev)?.Expenses;
            if (expenses != null) {
                onExpense(expenses);
            }

            onExpense(turn.Expenses);
        }

        DbRegionStatisticsItem regionItem(DbRegion region, string code, StatisticsCategory category) {
            var list = region.Statistics;

            var item = list.FirstOrDefault(x => x.Code == code && x.Category == category);
            if (item == null) {
                item = new DbRegionStatisticsItem {
                    PlayerId = turn.PlayerId,
                    TurnNumber = turn.TurnNumber,
                    RegionId = region.Id,
                    Category = category,
                    Code = code
                };
                list.Add(item);
            }

            return item;
        }

        DbTurnStatisticsItem turnItem(string code, StatisticsCategory category) {
            var list = turn.Statistics;

            var item = list.FirstOrDefault(x => x.Code == code && x.Category == category);
            if (item == null) {
                item = new DbTurnStatisticsItem {
                    PlayerId = turn.PlayerId,
                    TurnNumber = turn.TurnNumber,
                    Category = category,
                    Code = code
                };
                list.Add(item);
            }

            return item;
        }

        void updateItem(DbEvent ev, StatisticsCategory category) {
            var code = ev.ItemCode;
            var amount = ev.Amount ?? 0;

            turnItem(code, category).Amount += amount;

            var regionId = ev.RegionId;
            if (regionId != null && regions.TryGetValue(regionId, out var region)) {
                regionItem(region, code, category).Amount += amount;
            }
        }

        void produced(DbEvent ev) => updateItem(ev, StatisticsCategory.Produced);
        void bought(DbEvent ev) => updateItem(ev, StatisticsCategory.Bought);
        void sold(DbEvent ev) => updateItem(ev, StatisticsCategory.Sold);
        void consumed(DbEvent ev) => updateItem(ev, StatisticsCategory.Consumed);

        var events = db.Events
            .AsNoTracking()
            .InTurn(turn)
            .AsAsyncEnumerable();

        await foreach (var ev in events) {
            var amount = ev.Amount ?? 0;
            var price = ev.ItemPrice ?? 0;

            switch (ev.Category) {
                case EventCategory.Pillage:
                    income(ev, x => x.Pillage += amount);
                    break;

                case EventCategory.Tax:
                    income(ev, x => x.Tax += amount);
                    break;

                case EventCategory.Entertain:
                    income(ev, x => x.Entertain += amount);
                    break;

                case EventCategory.Work:
                    income(ev, x => x.Work += amount);
                    break;

                case EventCategory.Sell:
                    income(ev, x => x.Trade += amount * price);
                    sold(ev);
                    break;

                case EventCategory.Buy:
                    expenses(ev, x => x.Trade += amount * price);
                    bought(ev);
                    break;

                case EventCategory.Produce:
                    produced(ev);
                    break;

                case EventCategory.Claim:
                    income(ev, x => x.Claim += amount);
                    break;

                case EventCategory.Cast:
                    income(ev, x => x.Entertain += amount);
                    break;

                case EventCategory.Consume:
                    if (ev.ItemCode == null) {
                        expenses(ev, x => x.Consume += amount);
                    }
                    else {
                        consumed(ev);
                    }
                    break;

                case EventCategory.Withdraw:
                    income(ev, x => x.Entertain += amount);
                    break;

                case EventCategory.Give:
                case EventCategory.Study:
                case EventCategory.Unknown:
                    // do nothing
                    break;
            }
        }

        // Remove empty statistic items
        foreach (var region in regions.Values) {
            foreach (var item in region.Statistics) {
                if (item.Amount == 0) {
                    db.RegionStatistics.Remove(item);
                }
            }
        }

        foreach (var item in turn.Statistics) {
            if (item.Amount == 0) {
                db.TurnStatistics.Remove(item);
            }
        }
    }
}
