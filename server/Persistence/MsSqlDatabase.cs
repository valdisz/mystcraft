namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore;

public class MsSqlDatabase : Database
{
    public MsSqlDatabase()
    {
    }

    public MsSqlDatabase(DbContextOptions options) : base(options)
    {
    }
}
