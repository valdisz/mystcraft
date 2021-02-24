namespace atlantis.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using atlantis.Model;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

    public class Database : DbContext {
        public Database(DbContextOptions<Database> options)
            : base(options) {
        }

        public DbSet<DbGame> Games { get; set; }
        public DbSet<DbTurn> Turns { get; set; }
        public DbSet<DbReport> Reports { get; set; }
        public DbSet<DbFaction> Factions { get; set; }
        public DbSet<DbEvent> Events { get; set; }
        public DbSet<DbRegion> Regions { get; set; }
        public DbSet<DbStructure> Structures { get; set; }
        public DbSet<DbUnit> Units { get; set; }

        private readonly ValueConverter<List<string>, string> splitStringConverter = new ValueConverter<List<string>, string>(
            v => string.Join(";", v),
            v => v.Split(new[] { ';' }).ToList()
        );

        private readonly ValueConverter<List<Direction>, string> directionListConverter = new ValueConverter<List<Direction>, string>(
            v => string.Join(";", v.ToString()),
            v => v.Split(new[] { ';' })
                .Select(x => Enum.Parse<Direction>(x, true))
                .ToList()
        );

        protected override void OnModelCreating(ModelBuilder model) {
            model.Entity<DbGame>(t => {
                t.HasMany<DbReport>(x => x.Reports)
                    .WithOne(x => x.Game)
                    .HasForeignKey(x => x.GameId)
                    .IsRequired();

                t.HasMany<DbTurn>(x => x.Turns)
                    .WithOne(x => x.Game)
                    .HasForeignKey(x => x.GameId)
                    .IsRequired();
            });

            model.Entity<DbTurn>(t => {
                t.HasMany<DbReport>(x => x.Reports)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired();

                t.HasMany<DbRegion>(x => x.Regions)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired();

                t.HasMany(x => x.Factions)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired();

                t.HasMany(x => x.Events)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired();

                t.HasMany(x => x.Structures)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired();

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .IsRequired();
            });

            model.Entity<DbRegion>(t => {
                t.HasMany(x => x.Units)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => x.RegionId)
                    .IsRequired();

                t.HasMany(x => x.Structures)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => x.RegionId)
                    .IsRequired();

                t.OwnsOne(p => p.Settlement, a => {
                    a.Property(x => x.Size).HasConversion<string>();
                });

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
                    a.HasKey("RegionId", nameof(DbExit.Direction));
                    a.Property(x => x.Direction).HasConversion<string>();

                    a.OwnsOne(x => x.Settlement, b => {
                        b.Property(x => x.Size).HasConversion<string>();
                    });
                });
            });

            model.Entity<DbFaction>(t => {
                t.HasMany(x => x.Events)
                    .WithOne(x => x.Faction)
                    .HasForeignKey(x => x.FactionId)
                    .IsRequired();

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Faction)
                    .HasForeignKey(x => x.FactionId);
            });

            model.Entity<DbEvent>(t => {
                t.Property(x => x.Type).HasConversion<string>();
            });

            model.Entity<DbStructure>(t => {
                t.Property(p => p.Flags)
                    .HasConversion(splitStringConverter);

                t.Property(p => p.SailDirections)
                    .HasConversion(directionListConverter);

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Structure)
                    .HasForeignKey(x => x.StrcutureId);

                t.OwnsOne(p => p.Load);
                t.OwnsOne(p => p.Sailors);

                t.OwnsMany(p => p.Contents, a => {
                    a.WithOwner().HasForeignKey("StructureId");
                    a.ToTable("Structures_Contents");
                    a.HasKey("StructureId", nameof(DbFleetContent.Type));
                });
            });

            model.Entity<DbUnit>(t => {
                t.Property(p => p.Flags)
                    .HasConversion(splitStringConverter);

                t.OwnsMany(p => p.Items, a => {
                    a.WithOwner().HasForeignKey("UnitId");
                    a.ToTable("Unit_Items");
                    a.HasKey("UnitId", nameof(DbItem.Code));
                });

                t.OwnsOne(p => p.Capacity);

                t.OwnsMany(p => p.Skills, a => {
                    a.WithOwner().HasForeignKey("UnitId");
                    a.ToTable("Unit_Skills");
                    a.HasKey("UnitId", nameof(DbSkill.Code));
                });

                t.OwnsMany(p => p.CanStudy, a => {
                    a.WithOwner().HasForeignKey("UnitId");
                    a.ToTable("Unit_CanStudy");
                    a.HasKey("UnitId", nameof(DbSkill.Code));
                });

                t.OwnsOne(p => p.ReadyItem);

                t.OwnsOne(p => p.CombatSpell);
            });
        }
    }
}
