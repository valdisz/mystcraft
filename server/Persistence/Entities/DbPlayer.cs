namespace advisor.Persistence;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// This entity represents a player faction in a game.
/// </summary>
public class DbPlayer : IsAggregateRoot, InGameContext, WithCreationTime, WithUpdateTime {
    [Key]
    public long Id { get; set; }

    [HotChocolate.GraphQLIgnore]
    public long? UserId { get; set; }

    [HotChocolate.GraphQLIgnore]
    public long GameId { get; set; }

    public int Number { get; set; }

    public bool IsClaimed => UserId != null && Password != null;

    /// <summary>
    /// Player faction name (not user name). Will be equal to the last turn faction name.
    /// </summary>
    [MaxLength(Size.LABEL)]
    public string Name { get; set; }

    public int? LastTurnNumber { get; set; }

    public int? NextTurnNumber { get; set; }

    [MaxLength(Size.PASSWORD)]
    public string Password { get; set; }

    public bool IsQuit { get; set; }

    [HotChocolate.GraphQLIgnore]
    public DbUser User { get;set; }

    [HotChocolate.GraphQLIgnore]
    public DbGame Game { get;set; }

    // [GraphQLIgnore]
    public List<DbReport> Reports { get; set; } = new List<DbReport>();

    [HotChocolate.GraphQLIgnore]
    public List<DbPlayerTurn> Turns { get; set; } = new List<DbPlayerTurn>();

    [HotChocolate.GraphQLIgnore]
    public List<DbAdditionalReport> AdditionalReports { get; set; } = new List<DbAdditionalReport>();

    [HotChocolate.GraphQLIgnore]
    public List<DbAllianceMember> AllianceMembererships { get; set; } = new ();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public static DbPlayer CreateLocal(DbRegistration reg, int number)
        => new DbPlayer {
            GameId = reg.GameId,
            UserId = reg.UserId,
            Name = reg.Name,
            Number = number,
            Password = reg.Password
        };

    public static DbPlayer CreateRemote(int number, string name)
        => new DbPlayer {
            Number = number,
            Name = name
        };

    public Either<Error, DbPlayer> Claim(long userId, string password) {
        if (IsClaimed) {
            return Left(Error.New("Player already claimed."));
        }

        UserId = userId;
        Password = password;

        return Right(this);
    }

    public Either<Error, DbPlayer> Quit() {
        if (IsQuit) {
            return Left(Error.New("Player already quitted."));
        }

        IsQuit = true;

        return Right(this);
    }
}

public class DbPlayerConfiguration : IEntityTypeConfiguration<DbPlayer> {
    public DbPlayerConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbPlayer> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.HasMany(x => x.AdditionalReports)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.AllianceMembererships)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Reports)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbOrders>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbPlayerTurn>(x => x.Turns)
            .WithOne(x => x.Player)
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbTurnStatisticsItem>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbRegionStatisticsItem>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<DbTreasuryItem>()
            .WithOne()
            .HasForeignKey(x => x.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
