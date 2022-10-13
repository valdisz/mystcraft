namespace advisor.Persistence;

using System;
using HotChocolate;

public class DbSharingOptions {
    public bool AdvancedResources { get; set; }
    public bool Structures { get; set; }
    public bool Units { get; set; }
}

public class DbAllianceMember : InPlayerContext {
    [GraphQLIgnore]
    public long AllianceId { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }

    public bool ShareMap { get; set; }
    public bool TeachMages { get; set; }
    public bool Owner { get; set; }
    public bool CanInvite { get; set; }

    [GraphQLIgnore]
    public DbAlliance Alliance { get; set; }

    [GraphQLIgnore]
    public DbPlayer Player { get; set; }
}
