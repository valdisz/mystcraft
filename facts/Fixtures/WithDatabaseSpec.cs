namespace advisor.facts.Fixtures;

using advisor.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public abstract class WithDatabaseSpec : IAsyncLifetime {
    protected WithDatabaseSpec() {
        Connection = new SqliteConnection("Data Source=:memory:");
        var builder = new DbContextOptionsBuilder();
        builder.UseSqlite(Connection);

        Db = new SQLiteDatabase(builder.Options);
    }

    public async Task InitializeAsync() {
        await Connection.OpenAsync();
        await Db.Database.EnsureCreatedAsync();

        await Program.AddSeedDataAsync(Db);
    }

    public async Task DisposeAsync() {
        await Db.DisposeAsync();
        await Connection.CloseAsync();
        await Connection.DisposeAsync();
    }

    protected SqliteConnection Connection { get; }
    protected Database Db { get;}
}
