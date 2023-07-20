namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public interface WithCreator {
    long? CreatedByUserId { get; set; }
    DbUser CreatedBy { get; set; }
}

public static class Creator<T>
    where T : class, WithCreator {
    public static void Configure(Database db, EntityTypeBuilder<T> builder) {
        builder.HasOne(x => x.CreatedBy)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId);
    }
}
