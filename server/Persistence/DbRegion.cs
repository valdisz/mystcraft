namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
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

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();

        [GraphQLIgnore]
        public List<DbStructure> Structures { get; set; } = new List<DbStructure>();

        [GraphQLIgnore]
        public List<DbRegionStats> Stats { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new ();

        public static string GetUID(int x, int y, int z) => $"{x} {y} {z}";
    }
}
