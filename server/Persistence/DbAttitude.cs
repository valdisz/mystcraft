namespace advisor.Persistence;

using advisor.Model;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbAttitude : InFactionContext {
    [GraphQLIgnore]
    public long PlayerId { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public int FactionNumber { get; set; }

    [GraphQLName("factionNumber")]
    public int TargetFactionNumber { get; set; }

    public Stance Stance { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn{ get; set; }

    [GraphQLIgnore]
    public DbFaction Faction { get; set; }
}

public class DbAttitudeConfiguration : IEntityTypeConfiguration<DbAttitude> {
    public DbAttitudeConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbAttitude> builder) {
        builder.HasKey(x => new { x.PlayerId, x.TurnNumber, x.FactionNumber, x.TargetFactionNumber });

        builder.Property(x => x.Stance).HasConversion<string>();
    }
}
