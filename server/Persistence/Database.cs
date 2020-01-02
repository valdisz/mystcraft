namespace atlantis.Persistence
{
    using Microsoft.EntityFrameworkCore;

    public class Database : DbContext {
        public Database(DbContextOptions<Database> options)
            : base(options)
        {
        }

        public DbSet<DbGame> Games { get; set; }
        public DbSet<DbTurn> Turns { get; set; }
        public DbSet<DbRegion> Regions { get; set; }
        public DbSet<DbUnit> Units { get; set; }
        public DbSet<DbStructure> Structures { get; set; }

        protected override void OnModelCreating(ModelBuilder model) {

        }
    }

    public class DbGame {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class DbTurn {
        public long Id { get; set; }
        public long GameId { get; set; }
        public int Number { get; set; }
    }

    public class DbRegion {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public long Number { get; set; }
    }

    public class DbUnit {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public long Number { get; set; }
    }

    public class DbStructure {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public long Number { get; set; }
    }
}
