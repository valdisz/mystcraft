namespace advisor.Persistence;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// TODO: add time when user last time accessed the game
public class DbUser : WithCreationTime, WithUpdateTime {
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    [GraphQLIgnore]
    [MaxLength(Size.SALT)]
    public string Salt { get; set; }

    [GraphQLIgnore]
    public DigestAlgorithm Algorithm { get; set; }

    [GraphQLIgnore]
    [MaxLength(Size.DIGEST)]
    public string Digest { get; set; }

    [Authorize(Policy = Policies.UserManagers)]
    public DateTimeOffset CreatedAt { get; set; }

    [Authorize(Policy = Policies.UserManagers)]
    public DateTimeOffset UpdatedAt { get; set; }

    [Authorize(Policy = Policies.UserManagers)]
    public DateTimeOffset LastVisitAt { get; set; }

    [Authorize(Policy = Policies.UserManagers)]
    public List<string> Roles { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbLoginAttempt> LoginAttempts { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbUserIdentity> Identities { get; set; } = new ();

    public List<DbUserEmail> Emails { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbRegistration> Registrations { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbPlayer> Players { get; set; } = new ();
}

public class DbUserIdentity : WithCreationTime, WithUpdateTime {
    public long Id { get; set; }
    public long UserId { get; set; }

    [MaxLength(Size.PROVIDER)]
    public string Provider { get; set; }

    /// <summary>
    /// The token that uniquely identifies the user in the provider
    /// </summary>
    [Required]
    [MaxLength(Size.PROVIDER_TOKEN)]
    public string Token { get; set; }

    /// <summary>
    /// Provider specific information about user identity.
    /// </summary>
    public byte[] Details { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public DbUser User { get; set; }
}

public class DbUserIdentityConfiguration : IEntityTypeConfiguration<DbUserIdentity> {
    public DbUserIdentityConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbUserIdentity> builder) {
        throw new NotImplementedException();
    }
}

public class DbUserEmail : WithCreationTime, WithUpdateTime {
    public long Id { get; set; }
    public long UserId { get; set; }

    public bool Disabled { get; set; }

    public bool Primary { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    [Required]
    [MaxLength(Size.EMAIL)]
    public string Email { get; set; }

    [MaxLength(Size.EMAIL_VERIFICATION_CODE)]
    public string VerificationCode { get; set; }

    public DateTimeOffset? VerificationCodeExpiresAt { get; set; }
    public DateTimeOffset? EmailVerifiedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public DbUser User { get; set; }
}

public class DbUserEmailConfiguration : IEntityTypeConfiguration<DbUserEmail> {
    public DbUserEmailConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbUserEmail> builder) {
        builder.HasQueryFilter(x => x.DeletedAt == null);

        builder.HasIndex(x => new { x.Email })
            .IsUnique();
    }
}

public enum LoginOutcome {
    FAILURE = 0,
    SUCCESS = 1,
}

public class DbLoginAttempt {
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? UserIdentityId { get; set; }

    public LoginOutcome Outcome { get; set; }

    [MaxLength(Size.IP_ADDRESS)]
    public string IpAddress { get; set; }

    [MaxLength(Size.IP_ADDRESS_FAMILY)]
    public string IpAddressFamily { get; set; }

    [MaxLength(Size.HTTP_HEADER)]
    public string Referer { get; set; }

    [MaxLength(Size.HTTP_HEADER)]
    public string HttpVersion { get; set; }

    [MaxLength(Size.HTTP_HEADER)]
    public string UserAgent { get; set; }

    [MaxLength(Size.PROVIDER)]
    public string Provider { get; set; }

    [MaxLength(Size.COUNTRY)]
    public string Country { get; set; }

    [MaxLength(Size.CITY)]
    public string City { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public DbUser User { get; set; }
    public DbUserIdentity Identity { get; set; }
}

public class DbLoginAttemptConfiguration : IEntityTypeConfiguration<DbLoginAttempt> {
    public DbLoginAttemptConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbLoginAttempt> builder) {
    }
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

        CreationTime<DbUser>.Configure(db, builder);
        UpdateTime<DbUser>.Configure(db, builder);
    }
}
