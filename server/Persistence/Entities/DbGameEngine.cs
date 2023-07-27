namespace advisor.Persistence;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbGameEngine : WithAudit {
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    [MaxLength(Size.LONG_DESCRIPTION)]
    public string Description { get; set; }

    public bool Remote { get; set; }

    [MaxLength(Size.SYMBOL)]
    public string RemoteApi { get; set; }

    [MaxLength(Size.URL)]
    public string RemoteUrl { get; set; }

    [MaxLength(Size.MAX_TEXT)]
    public string RemoteOptions { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    public DateTimeOffset UpdatedAt { get; set; }

    public long? CreatedByUserId { get; set; }
    public long? UpdatedByUserId { get; set; }

    [GraphQLIgnore]
    public byte[] Engine { get; set; }

    [GraphQLIgnore]
    public byte[] Ruleset { get; set; }

    [GraphQLIgnore]
    public List<DbGame> Games { get; set; } = new ();

    public DbUser CreatedBy { get; set; }
    public DbUser UpdatedBy { get; set; }

    public static DbGameEngine NewLocal(string name, string description, byte[] engine, byte[] ruleset) =>
        new DbGameEngine {
            Name        = name,
            Description = description,
            Engine      = engine,
            Ruleset     = ruleset
        };

    public static DbGameEngine NewRemote(string name, string description, string api, string url, string options) =>
        new DbGameEngine {
            Name          = name,
            Description   = description,
            Remote        = true,
            RemoteApi     = api,
            RemoteUrl     = url,
            RemoteOptions = options
        };
}

public class DbGameEngineConfiguration : IEntityTypeConfiguration<DbGameEngine> {
    public DbGameEngineConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbGameEngine> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.Engine)
            .HasCompression();

        builder.Property(x => x.Ruleset)
            .HasCompression();

        builder.HasMany(x => x.Games)
            .WithOne(x => x.Engine)
            .HasForeignKey(x => x.EngineId)
            .IsRequired(false);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        Audit<DbGameEngine>.Configure(db, builder);
    }
}
