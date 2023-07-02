namespace advisor.Persistence;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public record RegionId(long PlayerId, int TurnNumber, int X, int Y, int Z) {
    private static readonly Regex PATTERN = new Regex(@"^(\d+) \((\d+),(\d+),(\d+)\)\@(\d+)$", RegexOptions.Compiled);

    public static RegionId CreateFrom(DbRegion region) => new RegionId(region.PlayerId, region.TurnNumber, region.X, region.Y, region.Z);

    public static RegionId CreateFrom(string s) {
        var m = PATTERN.Match(s);
        if (!m.Success) {
            return null;
        }

        return new RegionId(
            PlayerId: long.Parse(m.Groups[1].Value),
            TurnNumber: int.Parse(m.Groups[5].Value),
            X: int.Parse(m.Groups[2].Value),
            Y: int.Parse(m.Groups[3].Value),
            Z: int.Parse(m.Groups[4].Value)
        );
    }

    public override string ToString() =>  $"{PlayerId} ({X},{Y},{Z})@{TurnNumber}";
}

public class DbRegion : InTurnContext, IStatistics<DbRegionStatisticsItem> {
    [GraphQLIgnore]
    public string PublicId => RegionId.CreateFrom(this).ToString();

    [GraphQLIgnore]
    [Required]
    [MaxLength(Size.REGION_ID)]
    public string Id { get; set; }

    public static string MakeId(int x, int y, int z) => $"{x},{y},{z}";

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    // Full region report was obtained
    public bool Explored { get; set; }

    // Last turn OWN unit was in the region
    public int? LastVisitedAt { get; set; }

    [Required]
    [MaxLength(Size.LEVEL)]
    public string Label { get; set; }

    [Required]
    [MaxLength(Size.PROVINCE)]
    public string Province { get; set; }

    [Required]
    [MaxLength(Size.TERRAIN)]
    public string Terrain { get; set; }

    public DbSettlement Settlement { get; set; }

    [Required]
    public int Population { get; set; }

    [MaxLength(Size.RACE)]
    public string Race { get; set; }

    [Required]
    public int Entertainment { get; set; }

    [Required]
    public int Tax { get; set; }

    [Required]
    public double Wages { get; set; }

    [Required]
    public int TotalWages { get; set; }

    public int? Gate { get; set; }

    [GraphQLIgnore]
    public List<DbTradableItem> Markets { get; set; } = new List<DbTradableItem>();

    public IEnumerable<DbTradableItem> ForSale => Markets.Where(x => x.Market == Persistence.Market.FOR_SALE);
    public IEnumerable<DbTradableItem> Wanted => Markets.Where(x => x.Market == Persistence.Market.WANTED);

    public List<DbProductionItem> Produces { get; set; } = new List<DbProductionItem>();

    public DbIncome Income { get; set; } = new ();
    public DbExpenses Expenses { get; set; } = new ();
    public List<DbRegionStatisticsItem> Statistics { get; set; } = new ();

    public List<DbExit> Exits { get; set; } = new List<DbExit>();

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }

    [GraphQLIgnore]
    public List<DbUnit> Units { get; set; } = new List<DbUnit>();

    public List<DbStructure> Structures { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbEvent> Events { get; set; } = new ();

    public override string ToString() => Id;
}

public class DbRegionConfiguration : IEntityTypeConfiguration<DbRegion> {
    public DbRegionConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbRegion> builder) {
        builder.HasKey(x => new { x.PlayerId, x.TurnNumber, x.Id });

        builder.Ignore(x => x.ForSale);
        builder.Ignore(x => x.Wanted);

        builder.HasMany(x => x.Units)
            .WithOne(x => x.Region)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

        builder.HasMany(x => x.Structures)
            .WithOne(x => x.Region)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

        builder.HasMany(x => x.Statistics)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

        builder.HasMany(x => x.Events)
            .WithOne(x => x.Region)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

        builder.OwnsOne(p => p.Settlement, a => {
            a.Property(x => x.Size).HasConversion<string>();
        });

        builder.HasMany(p => p.Produces)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

        builder.HasMany(p => p.Markets)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

        builder.HasMany(x => x.Statistics)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

        builder.OwnsOne(x => x.Income);
        builder.OwnsOne(x => x.Expenses);
    }
}
