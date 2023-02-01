namespace advisor;

using System;
using System.Linq;
using advisor.Persistence;

public interface IPlayerTurnRepository
{
    IQueryable<DbPlayerTurn> PlayerTurns { get; }
    IQueryable<DbAdditionalReport> AditionalReports { get; }
    IQueryable<DbOrders> Orders { get; }
    IQueryable<DbFaction> Factions { get; }
    IQueryable<DbAttitude> Attitudes { get; }
    IQueryable<DbEvent> Events { get; }
    IQueryable<DbRegion> Regions { get; }
    IQueryable<DbProductionItem> Production { get; }
    IQueryable<DbTradableItem> Markets { get; }
    IQueryable<DbExit> Exits { get; }
    IQueryable<DbStructure> Structures { get; }
    IQueryable<DbUnit> Units { get; }
    IQueryable<DbUnitItem> Items { get; }
    IQueryable<DbBattle> Battles { get; }
}

public class PlayerTurnRepository : IPlayerTurnRepository
{
    public IQueryable<DbPlayerTurn> PlayerTurns => throw new NotImplementedException();
    public IQueryable<DbAdditionalReport> AditionalReports => throw new NotImplementedException();
    public IQueryable<DbOrders> Orders => throw new NotImplementedException();
    public IQueryable<DbFaction> Factions => throw new NotImplementedException();
    public IQueryable<DbAttitude> Attitudes => throw new NotImplementedException();
    public IQueryable<DbEvent> Events => throw new NotImplementedException();
    public IQueryable<DbRegion> Regions => throw new NotImplementedException();
    public IQueryable<DbProductionItem> Production => throw new NotImplementedException();
    public IQueryable<DbTradableItem> Markets => throw new NotImplementedException();
    public IQueryable<DbExit> Exits => throw new NotImplementedException();
    public IQueryable<DbStructure> Structures => throw new NotImplementedException();
    public IQueryable<DbUnit> Units => throw new NotImplementedException();
    public IQueryable<DbUnitItem> Items => throw new NotImplementedException();
    public IQueryable<DbBattle> Battles => throw new NotImplementedException();
}
