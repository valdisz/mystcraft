namespace atlantis.Persistence {
    using Microsoft.EntityFrameworkCore;

    public class Database : DbContext {
        public Database(DbContextOptions<Database> options)
            : base(options) {
        }

        public DbSet<DbGame> Games { get; set; }
        public DbSet<DbTurn> Turns { get; set; }
        public DbSet<DbRegion> Regions { get; set; }
        public DbSet<DbStructure> Structures { get; set; }
        public DbSet<DbUnit> Units { get; set; }

        protected override void OnModelCreating(ModelBuilder model) {
            model.Entity<DbGame>(t => {
                t.HasKey(x => x.Id);
            });

            model.Entity<DbTurn>(t => {
                t.HasKey(x => x.Id);

                t.HasOne(x => x.Game)
                    .WithMany(x => x.Turns)
                    .HasForeignKey(x => x.GameId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            model.Entity<DbRegion>(t => {
                t.HasKey(x => x.Id);

                t.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Turn)
                    .WithMany(x => x.Regions)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            model.Entity<DbStructure>(t => {
                t.HasKey(x => x.Id);

                t.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Turn)
                    .WithMany(x => x.Structures)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Region)
                    .WithMany(x => x.Structures)
                    .HasForeignKey(x => x.RegionId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
            });

            model.Entity<DbUnit>(t => {
                t.HasKey(x => x.Id);

                t.HasOne(x => x.Game)
                    .WithMany()
                    .HasForeignKey(x => x.GameId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Turn)
                    .WithMany(x => x.Units)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Region)
                    .WithMany(x => x.Units)
                    .HasForeignKey(x => x.RegionId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                t.HasOne(x => x.Structure)
                    .WithMany(x => x.Units)
                    .HasForeignKey(x => x.StrcutureId);
            });
        }
    }
}
