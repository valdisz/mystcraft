namespace advisor.Persistence;

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>
/// This interface is used to mark entities that have creation time.
/// </summary>
public interface WithCreationTime {
    DateTimeOffset CreatedAt { get; set; }
}

public static class CreationTime<T>
    where T : class, WithCreationTime {
    public static void Configure(Database db, EntityTypeBuilder<T> builder) {
        if (db.Provider == DatabaseProvider.SQLite) {
            builder
                .Property(e => e.CreatedAt)
                .HasConversion(new DateTimeOffsetToBinaryConverter());
        }
    }
}
