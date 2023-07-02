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
    public byte[] Ruleset { get; set; }

    [GraphQLIgnore]
    public List<DbGame> Games { get; set; } = new ();
}

public class DbGameEngineConfiguration : IEntityTypeConfiguration<DbGameEngine> {
    public DbGameEngineConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbGameEngine> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.HasMany(x => x.Games)
            .WithOne(x => x.Engine)
            .HasForeignKey(x => x.EngineId)
            .IsRequired(false);

        builder.HasIndex(x => x.Name)
            .IsUnique();
    }
}
