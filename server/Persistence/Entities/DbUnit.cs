namespace advisor.Persistence;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public record UnitId(long PlayerId, int TurnNumber, int UnitNumber);

public class DbUnit : InTurnContext {
    [GraphQLIgnore]
    [NotMapped]
    public string CompositeId => MakeId(this);

    public static string MakeId(DbUnit unit) => MakeId(unit.PlayerId, unit.TurnNumber, unit.Number);
    public static string MakeId(long playerId, int turnNumber, int unitNumber) => $"{playerId}/{turnNumber}/{unitNumber}";
    public static UnitId ParseId(string id) {
        var segments = id.Split("/");
        return new (
            long.Parse(segments[0]),
            int.Parse(segments[1]),
            int.Parse(segments[2])
        );
    }

    public static IQueryable<DbUnit> FilterById(IQueryable<DbUnit> q, UnitId id) {
        return q.Where(x =>
                x.PlayerId == id.PlayerId
            && x.TurnNumber == id.TurnNumber
            && x.Number == id.UnitNumber
        );
    }

    public int Number { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    [GraphQLIgnore]
    [MaxLength(Size.REGION_ID)]
    public string RegionId { get; set; }

    [GraphQLIgnore]
    [MaxLength(Size.STRUCTURE_ID)]
    public string StrcutureId { get; set; }

    public int? StructureNumber { get; set; }

    public int? FactionNumber { get; set; }

    public int Sequence { get; set; }

    [Required]
    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    [MaxLength(Size.DESCRIPTION)]
    public string Description { get; set; }

    [Required]
    public bool OnGuard { get; set; }

    [Required]
    public bool IsMage { get; set; }

    public List<string> Flags { get; set; } = new List<string>();

    public int? Weight { get; set; }

    [Required]
    public List<DbUnitItem> Items { get; set; } = new ();

    public DbCapacity Capacity { get; set; }

    public List<DbSkill> Skills { get; set; } = new List<DbSkill>();

    public List<string> CanStudy { get; set; } = new List<string>();

    [MaxLength(Size.ITEM_CODE)]
    public string ReadyItem { get; set; }

    [MaxLength(Size.ITEM_CODE)]
    public string CombatSpell { get; set; }

    public string Orders { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }

    [GraphQLIgnore]
    public DbRegion Region { get; set; }

    [GraphQLIgnore]
    public DbFaction Faction { get; set; }

    [GraphQLIgnore]
    public DbStructure Structure { get; set; }

    public DbStudyPlan StudyPlan { get; set; }

    [GraphQLIgnore]
    public List<DbEvent> Events { get; set; } = new ();
}

public class DbUnitConfiguration : IEntityTypeConfiguration<DbUnit> {
    public DbUnitConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbUnit> builder) {
        builder.HasKey(x => new { x.PlayerId, x.TurnNumber, x.Number });

        builder.Property(p => p.Flags)
            .HasConversionJson(db.Provider);

        builder.Property(p => p.CanStudy)
            .HasConversionJson(db.Provider);

        builder.Property(p => p.Skills)
            .HasConversionJson(db.Provider);

        builder.OwnsOne(p => p.Capacity);

        builder.HasMany(x => x.Events)
            .WithOne(x => x.Unit)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });

        builder.HasMany(p => p.Items)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });
    }
}
