namespace advisor.Persistence;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbPlayer : InGameContext {
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public long? UserId { get; set; }

    [GraphQLIgnore]
    public long GameId { get; set; }

    public int Number { get; set; }

    public bool IsClaimed => UserId != null && Password != null;

    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    public int? LastTurnNumber { get; set; }

    public int? NextTurnNumber { get; set; }

    [MaxLength(Size.PASSWORD)]
    public string Password { get; set; }

    public bool IsQuit { get; set; }

    [GraphQLIgnore]
    public DbUser User { get;set; }

    [GraphQLIgnore]
    public DbGame Game { get;set; }

    // [GraphQLIgnore]
    public List<DbReport> Reports { get; set; } = new List<DbReport>();

    [GraphQLIgnore]
    public List<DbPlayerTurn> Turns { get; set; } = new List<DbPlayerTurn>();

    [GraphQLIgnore]
    public List<DbAdditionalReport> AdditionalReports { get; set; } = new List<DbAdditionalReport>();

    [GraphQLIgnore]
    public List<DbAllianceMember> AllianceMembererships { get; set; } = new ();
}

public class DbPlayerConfiguration : IEntityTypeConfiguration<DbPlayer> {
    public DbPlayerConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbPlayer> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.HasMany(x => x.AdditionalReports)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AllianceMembererships)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reports)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbOrders>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbPlayerTurn>(x => x.Turns)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbTurnStatisticsItem>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbRegionStatisticsItem>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbTreasuryItem>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
