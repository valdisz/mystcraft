namespace advisor;

using System;
using System.Linq;
using advisor.Persistence;

public interface IAllianceRepository
{
    IQueryable<DbAlliance> Alliances { get; }
    IQueryable<DbAllianceMember> AllianceMembers { get; }
}

public class AllianceRepository : IAllianceRepository
{
    public IQueryable<DbAlliance> Alliances => throw new NotImplementedException();
    public IQueryable<DbAllianceMember> AllianceMembers => throw new NotImplementedException();
}
