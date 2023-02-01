namespace advisor.Persistence;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using advisor.Model;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StructureId = System.ValueTuple<long, int, string>;

public class DbStructure : InTurnContext {
    [GraphQLIgnore]
    [NotMapped]
    public StructureId CompositeId => MakeId(this);

    public static StructureId MakeId(long playerId, int turnNumber, string structureId) => (playerId, turnNumber, structureId);
    public static StructureId MakeId(DbStructure structure) => (structure.PlayerId, structure.TurnNumber, structure.Id);

    public static IQueryable<DbStructure> FilterById(IQueryable<DbStructure> q, StructureId id) {
        var (playerId, turnNumber, structureId) = id;
        return q.Where(x =>
                x.PlayerId == playerId
            && x.TurnNumber == turnNumber
            && x.Id == structureId
        );
    }

    [MaxLength(Size.STRUCTURE_ID)]
    public string Id { get; set; }

    public static string MakeId(int number, string regionId) => IsShip(number) ? number.ToString() : $"{number}@{regionId}";

    public static bool IsBuilding(int number) => number <= GameConsts.MAX_BUILDING_NUMBER;

    public static bool IsShip(int number) => number > GameConsts.MAX_BUILDING_NUMBER;

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    [GraphQLIgnore]
    [MaxLength(Size.REGION_ID)]
    public string RegionId { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public int Sequence { get; set; }

    public int Number { get; set; }

    [Required]
    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    [Required]
    [MaxLength(Size.STRUCTURE)]
    public string Type { get; set; }

    [MaxLength(Size.DESCRIPTION)]
    public string Description { get; set; }

    public List<DbFleetContent> Contents { get; set; } = new List<DbFleetContent>();
    public List<string> Flags { get; set; } = new List<string>();
    public List<Direction> SailDirections { get; set; } = new List<Direction>();
    public int? Speed { get; set; }
    public int? Needs { get; set; }
    public DbTransportationLoad Load { get; set; }
    public DbSailors Sailors { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }

    [GraphQLIgnore]
    public DbRegion Region { get; set; }

    [GraphQLIgnore]
    public List<DbUnit> Units { get; set; } = new List<DbUnit>();
}

public class DbStructureConfiguration : IEntityTypeConfiguration<DbStructure> {
    public DbStructureConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbStructure> builder) {
        builder.HasKey(x => new { x.PlayerId, x.TurnNumber, x.Id });

        builder.Property(p => p.Flags)
            .HasConversionJson(db.Provider);

        builder.Property(p => p.SailDirections)
            .HasConversionJson(db.Provider);

        builder.Property(p => p.Contents)
            .HasConversionJson(db.Provider);

        builder.HasMany(x => x.Units)
            .WithOne(x => x.Structure)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.StrcutureId });

        builder.OwnsOne(p => p.Load);
        builder.OwnsOne(p => p.Sailors);
    }
}
