namespace advisor.Persistence {
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public enum DatabaseProvider {
        SQLite,
        PgSQL,
        MsSQL
    }

    public class DatabaseOptions {
        public string ConnectionString { get; set; }
        public DatabaseProvider Provider { get; set; }
        public bool IsProduction { get; set; }
    }

    public class PgSqlDatabase : Database {
        public PgSqlDatabase(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactor)
            : base(options, loggerFactor) {
                this.options.Provider = DatabaseProvider.PgSQL;
        }
    }

    public class MsSqlDatabase : Database {
        public MsSqlDatabase(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactor)
            : base(options, loggerFactor) {
                this.options.Provider = DatabaseProvider.MsSQL;
        }
    }

    public class SQLiteDatabase : Database {
        public SQLiteDatabase(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactor)
            : base(options, loggerFactor) {
                this.options.Provider = DatabaseProvider.SQLite;
        }
    }

    public abstract class Database : DbContext {
        protected Database(IOptionsSnapshot<DatabaseOptions> options, ILoggerFactory loggerFactory) {
            this.options = options.Value;
            this.loggerFactory = loggerFactory;
        }

        protected DatabaseOptions options;
        private readonly ILoggerFactory loggerFactory;

        public DbSet<DbUser> Users { get; set; }

        public DbSet<DbGame> Games { get; set; }
        public DbSet<DbPlayer> Players { get; set; }

        public DbSet<DbTurn> Turns { get; set; }
        public DbSet<DbReport> Reports { get; set; }
        public DbSet<DbFaction> Factions { get; set; }
        public DbSet<DbStat> Stats { get; set; }
        public DbSet<DbEvent> Events { get; set; }
        public DbSet<DbRegion> Regions { get; set; }
        public DbSet<DbExit> Exits { get; set; }
        public DbSet<DbStructure> Structures { get; set; }
        public DbSet<DbUnit> Units { get; set; }
        public DbSet<DbUnitItem> Items { get; set; }

        public DbSet<DbAlliance> Universities { get; set; }
        public DbSet<DbAllianceMember> UniversityMemberships { get; set; }
        public DbSet<DbStudyPlan> StudyPlans { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            switch (options.Provider) {
                case DatabaseProvider.SQLite:
                    optionsBuilder.UseSqlite(options.ConnectionString);
                    break;

                case DatabaseProvider.PgSQL:
                    optionsBuilder.UseNpgsql(options.ConnectionString);
                    break;

                case DatabaseProvider.MsSQL:
                    optionsBuilder.UseSqlServer(options.ConnectionString);
                    break;
            }

            optionsBuilder.UseLoggerFactory(loggerFactory);

            if (!options.IsProduction) {
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging();
            }

            if (options.IsProduction) {
                optionsBuilder.ConfigureWarnings(c => c.Log((RelationalEventId.CommandExecuting, LogLevel.Debug)));
            }
        }

        protected override void OnModelCreating(ModelBuilder model) {
            model.Entity<DbUser>(t => {
                t.Property(x => x.Algorithm).HasConversion<string>();

                t.Property(p => p.Roles)
                    .HasJsonConversion(options.Provider);

                t.HasMany(p => p.Players)
                    .WithOne(p => p.User)
                    .HasForeignKey(x => x.UserId);

                t.HasIndex(x => new { x.Email })
                    .IsUnique();
            });

            model.Entity<DbGame>(t => {
                t.Property(x => x.Type)
                    .HasConversion<string>();

                t.HasMany(p => p.Players)
                    .WithOne(p => p.Game)
                    .HasForeignKey(x => x.GameId);

                t.HasMany(x => x.Alliances)
                    .WithOne(x => x.Game)
                    .HasForeignKey(x => x.GameId);
            });

            model.Entity<DbPlayer>(t => {
                t.HasMany<DbReport>(x => x.Reports)
                    .WithOne(x => x.Player)
                    .HasForeignKey(x => x.PlayerId);

                t.HasMany<DbTurn>(x => x.Turns)
                    .WithOne(x => x.Player)
                    .HasForeignKey(x => x.PlayerId);

                t.HasMany(x => x.AllianceMembererships)
                    .WithOne(x => x.Player)
                    .HasForeignKey(x => x.PlayerId);
            });

            model.Entity<DbTurn>(t => {
                t.HasKey(x => new { x.PlayerId, x.Number });

                t.HasMany(x => x.Reports)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Regions)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Factions)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Events)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Structures)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Plans)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Stats)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber })
                    .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<DbRegion>(t => {
                t.HasKey(x => new { x.PlayerId, x.TurnNumber, x.Id });

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

                t.HasMany(x => x.Structures)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

                t.HasMany(x => x.Stats)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

                t.HasMany(x => x.Events)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.RegionId });

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
            });

            model.Entity<DbExit>(t => {
                t.HasKey(x => new { x.PlayerId, x.TurnNumber, x.OriginRegionId, x.TargetRegionId });

                t.Property(p => p.Direction).HasConversion<string>();

                t.HasOne(p => p.Target)
                    .WithMany()
                    .HasForeignKey(p => new { p.PlayerId, p.TurnNumber, p.TargetRegionId });

                t.HasOne(p => p.Origin)
                    .WithMany(p => p.Exits)
                    .HasForeignKey(p => new { p.PlayerId, p.TurnNumber, p.OriginRegionId });
            });

            model.Entity<DbFaction>(t => {
                t.HasKey(x => new { x.PlayerId, x.TurnNumber, x.Number });

                t.HasMany(x => x.Events)
                    .WithOne(x => x.Faction)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.FactionNumber });

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Faction)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.FactionNumber });

                t.HasMany(x => x.Stats)
                    .WithOne(x => x.Faction)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.FactionNumber });
            });

            model.Entity<DbEvent>(t => {
                t.Property(x => x.Type).HasConversion<string>();
                t.Property(x => x.Category).HasConversion<string>();
            });

            model.Entity<DbStat>(t => {
                t.OwnsOne(x => x.Income);

                t.OwnsMany(p => p.Production, a => {
                    a.WithOwner().HasForeignKey("StatId");
                    a.ToTable("Stats_Production");
                    a.HasKey("StatId", nameof(DbItem.Code));
                });
            });

            model.Entity<DbStructure>(t => {
                t.HasKey(x => new { x.PlayerId, x.TurnNumber, x.Id });

                t.Property(p => p.Flags)
                    .HasJsonConversion(options.Provider);

                t.Property(p => p.SailDirections)
                    .HasJsonConversion(options.Provider);

                t.Property(p => p.Contents)
                    .HasJsonConversion(options.Provider);

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Structure)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.StrcutureId });

                t.OwnsOne(p => p.Load);
                t.OwnsOne(p => p.Sailors);
            });

            model.Entity<DbUnit>(t => {
                t.HasKey(x => new { x.PlayerId, x.TurnNumber, x.Number });

                t.Property(p => p.Flags)
                    .HasJsonConversion(options.Provider);

                t.Property(p => p.CanStudy)
                    .HasJsonConversion(options.Provider);

                t.Property(p => p.Skills)
                    .HasJsonConversion(options.Provider);

                t.OwnsOne(p => p.Capacity);

                t.OwnsOne(p => p.ReadyItem);

                t.OwnsOne(p => p.CombatSpell);

                t.HasMany(x => x.Events)
                    .WithOne(x => x.Unit)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });

                t.HasMany(p => p.Items)
                    .WithOne(p => p.Unit)
                    .HasForeignKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });
            });

            model.Entity<DbUnitItem>(t => {
                t.HasKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber, x.Code });
            });

            model.Entity<DbStudyPlan>(t => {
                t.HasKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });

                t.Property(x => x.Teach)
                    .HasJsonConversion(options.Provider);

                t.OwnsOne(p => p.Target);

                t.HasOne(p => p.Unit)
                    .WithOne(p => p.Plan)
                    .HasForeignKey<DbStudyPlan>(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });
            });

            model.Entity<DbAlliance>(t => {
                t.HasMany(p => p.Members)
                    .WithOne(x => x.Alliance)
                    .HasForeignKey(x => x.AllianceId);
            });

            model.Entity<DbAllianceMember>(t => {
                t.HasKey(x => new { x.PlayerId, x.AllianceId });
            });
        }
    }
}
