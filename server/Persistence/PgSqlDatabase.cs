namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore;

public class PgSqlDatabase : Database
{
    public PgSqlDatabase()
    {
    }

    public PgSqlDatabase(DbContextOptions options) : base(options)
    {
    }
}
