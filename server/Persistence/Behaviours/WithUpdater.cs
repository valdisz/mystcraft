namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public interface WithUpdater {
    long? UpdatedByUserId { get; set; }
    DbUser UpdatedBy { get; set; }
}

public static class Updater<T>
    where T : class, WithUpdater {
    public static void Configure(Database db, EntityTypeBuilder<T> builder) {
        builder.HasOne(x => x.UpdatedBy)
            .WithMany()
            .HasForeignKey(x => x.UpdatedByUserId);
    }
}
