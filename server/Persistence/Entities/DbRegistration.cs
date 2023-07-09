namespace advisor.Persistence;

using System;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbRegistration : InGameContext, WithCreationTime {
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public long GameId { get; set; }

    [GraphQLIgnore]
    public long UserId { get; set; }

    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    [MaxLength(Size.PASSWORD)]
    public string Password { get; set; }

    [GraphQLIgnore]
    public DbUser User { get;set; }

    [GraphQLIgnore]
    public DbGame Game { get;set; }

    public DateTimeOffset CreatedAt { get; set; }

    public static DbRegistration Create(long userId, string name, string password) {
        var reg = new DbRegistration {
            Name = name,
            Password = password,
            UserId = userId
        };

        return reg;
    }
}

public class DbRegistrationConfiguration : IEntityTypeConfiguration<DbRegistration> {
    public DbRegistrationConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbRegistration> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        CreationTime<DbRegistration>.Configure(db, builder);
    }
}
