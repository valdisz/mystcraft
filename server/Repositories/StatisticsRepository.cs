namespace advisor;

using System;
using System.Linq;
using advisor.Persistence;

public interface IStatisticsRepository
{
    IQueryable<DbTurnStatisticsItem> TurnStatistics { get; }
    IQueryable<DbRegionStatisticsItem> RegionStatistics { get; }
    IQueryable<DbTreasuryItem> Treasury { get; }
}

public class StatisticsRepository : IStatisticsRepository
{
    public IQueryable<DbTurnStatisticsItem> TurnStatistics => throw new NotImplementedException();
    public IQueryable<DbRegionStatisticsItem> RegionStatistics => throw new NotImplementedException();
    public IQueryable<DbTreasuryItem> Treasury => throw new NotImplementedException();
}
