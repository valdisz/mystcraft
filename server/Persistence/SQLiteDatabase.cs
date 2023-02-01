namespace advisor.Persistence;

using Microsoft.EntityFrameworkCore;

public class SQLiteDatabase : Database
{
    public SQLiteDatabase()
    {
    }

    public SQLiteDatabase(DbContextOptions options) : base(options)
    {
    }
}
