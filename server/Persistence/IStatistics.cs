namespace advisor.Persistence;

using System.Collections.Generic;

public interface IStatistics<T> where T: DbStatisticsItem {
    DbIncome Income { get; set; }
    DbExpenses Expenses { get; set; }
    List<T> Statistics { get; set; }
}
