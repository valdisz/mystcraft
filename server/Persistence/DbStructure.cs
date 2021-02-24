namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using atlantis.Model;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [GraphQLName("Structure")]
    public class DbStructure {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public string UID => GetUID(Number, X, Y, Z);

        [GraphQLIgnore]
        public long TurnId { get; set; }

        [GraphQLIgnore]
        public long RegionId { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        [GraphQLIgnore]
        public int Sequence { get; set; }

        public int Number { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        public string Description { get; set; }

        public List<DbFleetContent> Contents { get; set; } = new List<DbFleetContent>();
        public List<string> Flags { get; set; } = new List<string>();
        public List<Direction> SailDirections { get; set; } = new List<Direction>();
        public int? Speed { get; set; }
        public int? Needs { get; set; }
        public DbTransportationLoad Load { get; set; }
        public DbSailors Sailors { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();

        public static string GetUID(int number, int x, int y, int z) => number > GameConsts.MAX_BUILDING_NUMBER
            ? $"[{number}]"
            : $"{DbRegion.GetUID(x, y, z)} [{number}]";
    }

    [Owned]
    public class DbFleetContent {
        public string Type { get; set; }
        public int Count { get; set; }
    }

    [Owned]
    public class DbTransportationLoad {
        public int Used { get; set; }
        public int Max { get; set; }
    }

    [Owned]
    public class DbSailors {
        public int Current { get; set; }
        public int Required { get; set; }
    }
}
