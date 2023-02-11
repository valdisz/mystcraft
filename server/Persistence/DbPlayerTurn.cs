namespace advisor.Persistence;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbPlayerTurn : InTurnContext, InGameContext, IStatistics<DbTurnStatisticsItem> {
    public static string CreateId(long playerId, int turnNumber) => $"{playerId}:{turnNumber}";
    public static string CreateId(DbPlayerTurn player) => CreateId(player.PlayerId, player.TurnNumber);
    public static (long playerId, int turnNumber) ParseId(string id) {
        var values = id.Split(":");
        return (
            long.Parse(values[0]),
            int.Parse(values[1])
        );
    }


    public string Id => CreateId(this);

    [GraphQLIgnore]
    public long GameId { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public int TurnNumber { get; set; }


    [Required]
    [MaxLength(Size.FACTION)]
    public string FactionName { get; set; }

    public int FactionNumber { get; set; }

    public int Unclaimed { get; set; }

    public DbIncome Income { get; set; } = new ();

    public DbExpenses Expenses { get; set; } = new ();

    // [UseOffsetPaging]
    public List<DbTurnStatisticsItem> Statistics { get; set; } = new ();

    // [UseOffsetPaging]
    public List<DbTreasuryItem> Treasury { get; set; } = new ();


    public DateTimeOffset? ReadyAt { get; set; }
    public DateTimeOffset? OrdersSubmittedAt { get; set; }
    public DateTimeOffset? TimesSubmittedAt { get; set; }

    public bool IsReady => ReadyAt.HasValue;
    public bool IsOrdersSubmitted => OrdersSubmittedAt.HasValue;
    public bool IsTimesSubmitted => TimesSubmittedAt.HasValue;

    public bool IsProcessed { get; set; }

    [GraphQLIgnore]
    public DbPlayer Player { get; set; }

    [GraphQLIgnore]
    public List<DbAdditionalReport> Reports { get; set; } = new List<DbAdditionalReport>();

    [GraphQLIgnore]
    public List<DbRegion> Regions { get; set; } = new List<DbRegion>();

    [GraphQLIgnore]
    public List<DbExit> Exits { get; set; } = new List<DbExit>();

    [GraphQLIgnore]
    public List<DbTradableItem> Markets { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbProductionItem> Production { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbFaction> Factions { get; set; } = new List<DbFaction>();

    [GraphQLIgnore]
    public List<DbAttitude> Attitudes { get; set; } = new List<DbAttitude>();

    [GraphQLIgnore]
    public List<DbEvent> Events { get; set; } = new List<DbEvent>();

    [GraphQLIgnore]
    public List<DbUnit> Units { get; set; } = new List<DbUnit>();

    [GraphQLIgnore]
    public List<DbUnitItem> Items { get; set; } = new List<DbUnitItem>();

    [GraphQLIgnore]
    public List<DbStructure> Structures { get; set; } = new List<DbStructure>();

    [GraphQLIgnore]
    public List<DbStudyPlan> Plans { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbBattle> Battles { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbOrders> Orders { get; set; } = new ();

    public static DbPlayerTurn Create(int factionNumber, string factionName, int turnNumber) {
        return new DbPlayerTurn {
            FactionNumber = factionNumber,
            FactionName = factionName,
            TurnNumber = turnNumber
        };
    }
}

public class DbPlayerTurnConfiguration : IEntityTypeConfiguration<DbPlayerTurn> {
    public DbPlayerTurnConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbPlayerTurn> t) {
        t.HasKey(x => new { x.PlayerId, x.TurnNumber });

        t.HasMany(x => x.Reports)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Regions)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Exits)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Markets)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Production)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Factions)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Attitudes)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Events)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Structures)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Units)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Plans)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Statistics)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Treasury)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Cascade);

        t.HasMany(x => x.Battles)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.HasMany(x => x.Orders)
            .WithOne(x => x.Turn)
            .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
            .OnDelete(DeleteBehavior.Restrict);

        t.OwnsOne(x => x.Income);
        t.OwnsOne(x => x.Expenses);
    }
}

