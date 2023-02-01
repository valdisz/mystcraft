namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using advisor.Model;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// [GraphQLName("Event")]
public class DbEvent : InFactionContext {
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public int FactionNumber { get; set; }

    [GraphQLName("regionCode")]
    [MaxLength(Size.REGION_ID)]
    public string RegionId { get; set; }

    [GraphQLIgnore]
    public int? UnitNumber { get; set; }

    [MaxLength(Size.LABEL)]
    public string UnitName { get; set; }

    [GraphQLIgnore]
    public int? MissingUnitNumber { get; set; }

    [Required]
    public EventType Type { get; set; }

    [Required]
    public EventCategory Category { get; set; } = EventCategory.Unknown;

    [Required]
    public string Message { get; set; }

    public int? Amount { get; set; }

    [MaxLength(Size.ITEM_CODE)]
    public string ItemCode { get; set; }

    [MaxLength(Size.ITEM_NAME)]
    public string ItemName { get; set; }

    public int? ItemPrice { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }

    [GraphQLIgnore]
    public DbFaction Faction { get; set; }

    [GraphQLIgnore]
    public DbRegion Region { get; set; }

    [GraphQLIgnore]
    public DbUnit Unit { get; set; }
}

public class DbEventConfiguration : IEntityTypeConfiguration<DbEvent> {
    public DbEventConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbEvent> builder) {
        builder.Property(x => x.Type).HasConversion<string>();
        builder.Property(x => x.Category).HasConversion<string>();
    }
}
