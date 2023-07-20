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

    [Required]
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public long? CreatedByUserId { get; set; }
    public long? UpdatedByUserId { get; set; }

    [Required]
    [GraphQLIgnore]
    public byte[] Contents { get; set; }

    [Required]
    [GraphQLIgnore]
    public byte[] Ruleset { get; set; }

    [GraphQLIgnore]
    public List<DbGame> Games { get; set; } = new ();

    public DbUser CreatedBy { get; set; }
    public DbUser UpdatedBy { get; set; }

    public static DbGameEngine New(string name, string description, byte[] contents, byte[] ruleset) =>
        new DbGameEngine {
            Name = name,
            Description = description,
            Contents = contents,
            Ruleset = ruleset
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

        builder.Property(x => x.Contents)
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
