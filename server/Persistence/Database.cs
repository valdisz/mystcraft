namespace advisor.Persistence
{
    using System;
    using System.Collections.Generic;
    using advisor.Model;
    using AutoMapper.Configuration;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
    using Microsoft.Extensions.Options;
    using Newtonsoft.Json;

    public class JsonListConverter<T> : ValueConverter<List<T>, string>
    {
        public JsonListConverter()
            : base(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<List<T>>(v)
            )
        {
        }
    }

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
        public PgSqlDatabase(IOptionsSnapshot<DatabaseOptions> options)
            : base(options) {
                this.options.Provider = DatabaseProvider.PgSQL;
        }
    }

    public class MsSqlDatabase : Database {
        public MsSqlDatabase(IOptionsSnapshot<DatabaseOptions> options)
            : base(options) {
                this.options.Provider = DatabaseProvider.MsSQL;
        }
    }

    public class SQLiteDatabase : Database {
        public SQLiteDatabase(IOptionsSnapshot<DatabaseOptions> options)
            : base(options) {
                this.options.Provider = DatabaseProvider.SQLite;
        }
    }

    public abstract class Database : DbContext {
        protected Database(IOptionsSnapshot<DatabaseOptions> options) {
            this.options = options.Value;
        }

        protected DatabaseOptions options;

        public DbSet<DbUser> Users { get; set; }

        public DbSet<DbGame> Games { get; set; }
        public DbSet<DbPlayer> Players { get; set; }

        public DbSet<DbTurn> Turns { get; set; }
        public DbSet<DbReport> Reports { get; set; }
        public DbSet<DbFaction> Factions { get; set; }
        public DbSet<DbEvent> Events { get; set; }
        public DbSet<DbRegion> Regions { get; set; }
        public DbSet<DbStructure> Structures { get; set; }
        public DbSet<DbUnit> Units { get; set; }

        public DbSet<DbUniversity> Universities { get; set; }
        public DbSet<DbUniversityMembership> UniversityMemberships { get; set; }
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

            if (!options.IsProduction) {
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder model) {
            model.Entity<DbUser>(t => {
                t.Property(x => x.Algorithm).HasConversion<string>();

                t.HasMany(p => p.Players)
                    .WithOne(p => p.User)
                    .HasForeignKey(x => x.UserId);

                t.OwnsMany(x => x.Roles, a => {
                    a.WithOwner().HasForeignKey("UserId");
                    a.ToTable("Users_Role");
                    a.HasKey("UserId", nameof(DbUserRole.Role));
                });

                t.HasIndex(x => new { x.Email })
                    .IsUnique();
            });

            model.Entity<DbGame>(t => {
                t.Property(x => x.Type)
                    .HasConversion<string>();

                t.HasMany(p => p.Players)
                    .WithOne(p => p.Game)
                    .HasForeignKey(x => x.GameId);

                t.HasMany(x => x.Universities)
                    .WithOne(x => x.Game)
                    .HasForeignKey(x => x.GameId);

                t.HasIndex(x => new { x.Name })
                    .IsUnique();
            });

            model.Entity<DbPlayer>(t => {
                t.HasMany<DbReport>(x => x.Reports)
                    .WithOne(x => x.Player)
                    .HasForeignKey(x => x.PlayerId);

                t.HasMany<DbTurn>(x => x.Turns)
                    .WithOne(x => x.Player)
                    .HasForeignKey(x => x.PlayerId);
            });

            model.Entity<DbTurn>(t => {
                t.HasMany<DbReport>(x => x.Reports)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany<DbRegion>(x => x.Regions)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Factions)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Events)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Structures)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasMany(x => x.Plans)
                    .WithOne(x => x.Turn)
                    .HasForeignKey(x => x.TurnId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            model.Entity<DbRegion>(t => {
                t.HasMany(x => x.Units)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => x.RegionId);

                t.HasMany(x => x.Structures)
                    .WithOne(x => x.Region)
                    .HasForeignKey(x => x.RegionId);

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
                    .HasForeignKey(x => x.FactionId);

                t.HasMany(x => x.Units)
                    .WithOne(x => x.Faction)
                    .HasForeignKey(x => x.FactionId);
            });

            model.Entity<DbEvent>(t => {
                t.Property(x => x.Type).HasConversion<string>();
            });

            model.Entity<DbStructure>(t => {
                t.Property(p => p.Flags)
                    .HasConversion(new JsonListConverter<string>());

                t.Property(p => p.SailDirections)
                    .HasConversion(new JsonListConverter<Direction>());

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
                    .HasConversion(new JsonListConverter<string>());

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

            model.Entity<DbUniversity>(t => {
                t.HasMany(p => p.Plans)
                    .WithOne(p => p.University)
                    .HasForeignKey(p => p.UniversityId);
            });

            model.Entity<DbStudyPlan>(t => {
                t.Property(x => x.Teach)
                    .HasConversion(new JsonListConverter<long>());

                t.OwnsOne(p => p.Target);

                t.HasOne(p => p.Unit)
                    .WithOne(p => p.Plan)
                    .HasForeignKey<DbStudyPlan>(p => p.UnitId);
            });

            model.Entity<DbUniversityMembership>(t => {
                t.HasKey(x => new { x.PlayerId, x.UniversityId });

                t.HasOne(x => x.Player)
                    .WithOne(x => x.UniversityMembership)
                    .HasForeignKey<DbUniversityMembership>(x => x.PlayerId)
                    .OnDelete(DeleteBehavior.Restrict);

                t.HasOne(x => x.University)
                    .WithMany(x => x.Members)
                    .HasForeignKey(x => x.UniversityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
