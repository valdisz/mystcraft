namespace advisor.Persistence;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using advisor.Model;

public class DbGame : IsAggregateRoot, WithAudit {
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    [MaxLength(Size.TYPE)]
    public GameType Type { get; set; }

    [MaxLength(Size.TYPE)]
    public GameStatus Status { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }
    [Required]
    public DateTimeOffset UpdatedAt { get; set; }
    public long? CreatedByUserId { get; set; }
    public long? UpdatedByUserId { get; set; }
    public DbUser CreatedBy { get; set; }
    public DbUser UpdatedBy { get; set; }

    [GraphQLIgnore]
    public long? EngineId { get; set; }

    [Required]
    public GameOptions Options { get; set; }


    public int? LastTurnNumber { get; set; }

    public int? NextTurnNumber { get; set; }


    [GraphQLIgnore]
    public List<DbPlayer> Players { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbRegistration> Registrations { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbAlliance> Alliances { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbTurn> Turns { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbArticle> Articles { get; set; } = new ();

    [GraphQLIgnore]
    public DbGameEngine Engine { get; set; }

    public static DbGame New(string name, long engineId, GameOptions options) {
        var game = new DbGame {
            Name = name,
            Type = GameType.LOCAL,
            Status = GameStatus.NEW,
            EngineId = engineId,
            // Ruleset = ruleset,
            Options = options,
            NextTurnNumber = 1
        };

        return game;
    }
}

public class DbGameConfiguration : IEntityTypeConfiguration<DbGame> {
    public DbGameConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbGame> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.Type)
            .HasConversion<string>();

        builder.Property(x => x.Options)
            .HasConversionJson(db.Provider);

        builder.HasMany(p => p.Players)
            .WithOne(p => p.Game)
            .HasForeignKey(x => x.GameId);

        builder.HasMany(x => x.Alliances)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId);

        builder.HasMany(x => x.Turns)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Articles)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Registrations)
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbReport>()
            .WithOne(x => x.Game)
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbPlayerTurn>()
            .WithOne()
            .HasForeignKey(x => x.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        Audit<DbGame>.Configure(db, builder);
    }
}
