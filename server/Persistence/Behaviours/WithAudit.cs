namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public interface WithAudit : WithCreator, WithUpdater, WithCreationTime, WithUpdateTime {

}

public static class Audit<T>
    where T : class, WithAudit {
    public static void Configure(Database db, EntityTypeBuilder<T> builder) {
        CreationTime<T>.Configure(db, builder);
        UpdateTime<T>.Configure(db, builder);
        Creator<T>.Configure(db, builder);
        Updater<T>.Configure(db, builder);
    }
}
