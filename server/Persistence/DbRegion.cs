namespace atlantis.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using atlantis.Model;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [GraphQLName("Region")]
    public class DbRegion {
        [Key]
        public long Id { get; set; }


        [GraphQLIgnore]
        public long TurnId { get; set; }

        [GraphQLIgnore]
        public string UID => GetUID(X, Y, Z);

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public int UpdatedAtTurn { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        public string Terrain { get; set; }

        public DbSettlement Settlement { get; set; }

        [Required]
        public int Population { get; set; }

        public string Race { get; set; }

        [Required]
        public int Entertainment { get; set; }

        [Required]
        public int Tax { get; set; }

        [Required]
        public double Wages { get; set; }

        [Required]
        public int TotalWages { get; set; }

        public List<DbTradableItem> ForSale { get; set; } = new List<DbTradableItem>();

        public List<DbTradableItem> Wanted { get; set; } = new List<DbTradableItem>();


        public List<DbItem> Products { get; set; } = new List<DbItem>();

        public List<DbExit> Exits { get; set; } = new List<DbExit>();

        public DbTurn Turn { get; set; }

        // public List<DbUnit> Units { get; set; }
        // public List<DbStructure> Structures { get; set; }

        public static string GetUID(int x, int y, int z) => $"{x} {y} {z}";
    }

    [Owned]
    public class DbSettlement {
        public string Name { get; set; }
        public SettlementSize Size { get; set; }
    }

    [Owned]
    public class DbExit {
        public DbExit() {

        }

        public DbExit(DbExit other) {
            this.Direction = other.Direction;
            this.Label = other.Label;
            this.Province = other.Province;
            this.Settlement = other.Settlement != null
                ? new DbSettlement {
                    Name = other.Settlement.Name,
                    Size = other.Settlement.Size
                }
                : null;
            this.Terrain = other.Terrain;
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
        }

        [GraphQLIgnore]
        public string RegionUID => DbRegion.GetUID(X, Y, Z);

        [Required]
        public Direction Direction { get; set; }

        [Required]
        public int X { get; set; }

        [Required]
        public int Y { get; set; }

        [Required]
        public int Z { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        public string Terrain { get; set; }

        public DbSettlement Settlement { get; set; }
    }
}
