namespace advisor.Persistence;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public enum DigestAlgorithm {
    SHA256
}

public class DbUser : WithCreationTime {
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(Size.EMAIL)]
    public string Email { get; set; }

    [GraphQLIgnore]
    [MaxLength(Size.SALT)]
    public string Salt { get; set; }

    [GraphQLIgnore]
    public DigestAlgorithm Algorithm { get; set; }

    [GraphQLIgnore]
    [MaxLength(Size.DIGEST)]
    public string Digest { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastLoginAt { get; set; }

    public List<string> Roles { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbRegistration> Registrations { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbPlayer> Players { get; set; } = new ();
}

public class DbUserConfiguration : IEntityTypeConfiguration<DbUser> {
    public DbUserConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbUser> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.Algorithm)
            .HasConversion<string>();

        builder.Property(p => p.Roles)
            .HasConversionJson(db.Provider);

        builder.HasMany(x => x.Registrations)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Players)
            .WithOne(p => p.User)
            .HasForeignKey(x => x.UserId)
            .IsRequired(false);

        builder.HasIndex(x => new { x.Email })
            .IsUnique();
    }
}
