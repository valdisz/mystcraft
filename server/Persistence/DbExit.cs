namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using advisor.Model;
using HotChocolate;
using HotChocolate.Types.Relay;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbExit : InTurnContext {
    [GraphQLIgnore]
    public long PlayerId { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    [Required]
    [MaxLength(Size.REGION_ID)]
    public string OriginRegionId { get; set; }

    [GraphQLName("targetRegion")]
    [Required]
    [MaxLength(Size.REGION_ID)]
    [ID("Region")]
    public string TargetRegionId { get; set; }

    [Required]
    public Direction Direction { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    [Required]
    [MaxLength(Size.LABEL)]
    public string Label { get; set; }

    [Required]
    [MaxLength(Size.PROVINCE)]
    public string Province { get; set; }

    [Required]
    [MaxLength(Size.TERRAIN)]
    public string Terrain { get; set; }

    public DbSettlement Settlement { get; set; }

    [GraphQLIgnore]
    public DbRegion Origin { get; set; }

    [GraphQLIgnore]
    public DbRegion Target { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }
}

public class DbExitConfiguration : IEntityTypeConfiguration<DbExit> {
    public DbExitConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbExit> builder) {
        builder.HasKey(x => new { x.PlayerId, x.TurnNumber, x.OriginRegionId, x.TargetRegionId });

        builder.Property(p => p.Direction).HasConversion<string>();

        builder.OwnsOne(p => p.Settlement, a => {
            a.Property(x => x.Size).HasConversion<string>();
        });

        builder.HasOne(p => p.Target)
            .WithMany()
            .HasForeignKey(p => new { p.PlayerId, p.TurnNumber, p.TargetRegionId });

        builder.HasOne(p => p.Origin)
            .WithMany(p => p.Exits)
            .HasForeignKey(p => new { p.PlayerId, p.TurnNumber, p.OriginRegionId });
    }
}
