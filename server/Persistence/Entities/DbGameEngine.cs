namespace advisor.Persistence;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbGameEngine : WithCreationTime {
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    [Required]
    [GraphQLIgnore]
    public byte[] Contents { get; set; }

    [Required]
    [GraphQLIgnore]
    public byte[] Ruleset { get; set; }

    [GraphQLIgnore]
    public List<DbGame> Games { get; set; } = new ();

    public static DbGameEngine New(string name, byte[] contents, byte[] ruleset) =>
        new DbGameEngine {
            Name = name,
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

        CreationTime<DbGameEngine>.Configure(db, builder);
    }
}