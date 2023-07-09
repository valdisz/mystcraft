namespace advisor.Persistence;

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// This interface is used to mark entities that have update time.
/// </summary>
public interface WithUpdateTime {
    DateTimeOffset UpdatedAt { get; set; }
}

public static class UpdateTime<T>
    where T : class, WithUpdateTime {
    public static void Configure(Database db, EntityTypeBuilder<T> builder) {
        if (db.Provider == DatabaseProvider.SQLite) {
            builder
                .Property(e => e.UpdatedAt)
                .HasConversion(new DateTimeOffsetToBinaryConverter());
        }
    }
}
