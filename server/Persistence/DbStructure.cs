namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;

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
            ? $"{number}"
            : $"{number}@{DbRegion.GetUID(x, y, z)}";
    }
}
