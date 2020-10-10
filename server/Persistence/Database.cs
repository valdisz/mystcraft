namespace atlantis.Persistence
{
    using Microsoft.EntityFrameworkCore;

    public class Database : DbContext {
        public Database(DbContextOptions<Database> options)
            : base(options) {
        }

        public DbSet<DbGame> Games { get; set; }
        public DbSet<DbReport> Reports { get; set; }
        public DbSet<DbTurn> Turns { get; set; }
        public DbSet<DbRegion> Regions { get; set; }
        // public DbSet<DbFaction> Factions { get; set; }
        // public DbSet<DbEvent> Events { get; set; }
        // public DbSet<DbStructure> Structures { get; set; }
        // public DbSet<DbUnit> Units { get; set; }

        protected override void OnModelCreating(ModelBuilder model) {
            model.Entity<DbGame>(t => {
                t.HasMany<DbReport>(x => x.Reports)
                    .WithOne(x => x.Game)
                    .HasForeignKey(x => x.GameId);

                t.HasMany<DbTurn>(x => x.Turns)
                    .WithOne(x => x.Game)
                    .HasForeignKey(x => x.GameId);
            });

            model.Entity<DbTurn>(t => {
                t.HasMany<DbReport>(x => x.Reports)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId);

                t.HasMany<DbRegion>(x => x.Regions)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId);
            });

            model.Entity<DbRegion>(t => {
                t.OwnsMany(p => p.ForSale, a => {
                    a.WithOwner().HasForeignKey("RegionId");
                    a.ToTable("Regions_ForSale");
                    a.HasKey("RegionId", nameof(DbTradableItem.Code));
                });

                t.OwnsMany(p => p.Wanted, a => {
                    a.WithOwner().HasForeignKey("RegionId");
                    a.ToTable("Regions_Wanted");
                    a.HasKey("RegionId", nameof(DbTradableItem.Code));
                });

                t.OwnsMany(p => p.Products, a => {
                    a.WithOwner().HasForeignKey("RegionId");
                    a.ToTable("Regions_Products");
                    a.HasKey("RegionId", nameof(DbItem.Code));
                });

                t.OwnsMany(p => p.Exits, a => {
                    a.WithOwner().HasForeignKey("RegionId");
                    a.ToTable("Regions_Exits");
                    a.HasKey("RegionId", nameof(DbRegionExit.RegionUID));
                });
            });

            // model.Entity<DbTurn>(t => {
            //     t.HasOne(x => x.Game)
            //         .WithMany(x => x.Turns)
            //         .HasForeignKey(x => x.GameId)
            //         .IsRequired();
            // });

            // model.Entity<DbFaction>(t => {
            //     t.HasOne(x => x.Game)
            //         .WithMany()
            //         .HasForeignKey(x => x.GameId)
            //         .IsRequired();

            //     t.HasOne(x => x.Turn)
            //         .WithMany(x => x.Factions)
            //         .HasForeignKey(x => x.TurnId)
            //         .IsRequired();
            // });

            // model.Entity<DbEvent>(t => {
            //     t.HasOne(x => x.Game)
            //         .WithMany()
            //         .HasForeignKey(x => x.GameId)
            //         .IsRequired();

            //     t.HasOne(x => x.Turn)
            //         .WithMany(x => x.Events)
            //         .HasForeignKey(x => x.TurnId)
            //         .IsRequired();

            //     t.HasOne(x => x.Faction)
            //         .WithMany(x => x.Events)
            //         .HasForeignKey(x => x.FactionId)
            //         .IsRequired();
            // });

            // model.Entity<DbRegion>(t => {
            //     t.HasOne(x => x.Game)
            //         .WithMany()
            //         .HasForeignKey(x => x.GameId)
            //         .IsRequired();

            //     t.HasOne(x => x.Turn)
            //         .WithMany(x => x.Regions)
            //         .HasForeignKey(x => x.TurnId)
            //         .IsRequired();
            // });

            // model.Entity<DbStructure>(t => {
            //     t.HasOne(x => x.Game)
            //         .WithMany()
            //         .HasForeignKey(x => x.GameId)
            //         .IsRequired();

            //     t.HasOne(x => x.Turn)
            //         .WithMany(x => x.Structures)
            //         .HasForeignKey(x => x.TurnId)
            //         .IsRequired();

            //     t.HasOne(x => x.Region)
            //         .WithMany(x => x.Structures)
            //         .HasForeignKey(x => x.RegionId)
            //         .IsRequired();
            // });

            // model.Entity<DbUnit>(t => {
            //     t.HasOne(x => x.Game)
            //         .WithMany()
            //         .HasForeignKey(x => x.GameId)
            //         .IsRequired();

            //     t.HasOne(x => x.Turn)
            //         .WithMany(x => x.Units)
            //         .HasForeignKey(x => x.TurnId)
            //         .IsRequired();

            //     t.HasOne(x => x.Region)
            //         .WithMany(x => x.Units)
            //         .HasForeignKey(x => x.RegionId)
            //         .IsRequired();

            //     t.HasOne(x => x.Structure)
            //         .WithMany(x => x.Units)
            //         .HasForeignKey(x => x.StrcutureId);
            // });
        }
    }
}
