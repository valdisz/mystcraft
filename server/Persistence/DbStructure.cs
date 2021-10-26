namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;

    [GraphQLName("Structure")]
    public class DbStructure : InTurnContext {
        [MaxLength(24)]
        public string Id { get; set; }

        public static string MakeId(int number, string regionId) => IsShip(number) ? number.ToString() : $"{number}@{regionId}";

        public static bool IsBuilding(int number) => number <= GameConsts.MAX_BUILDING_NUMBER;

        public static bool IsShip(int number) => number > GameConsts.MAX_BUILDING_NUMBER;

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        [MaxLength(14)]
        public string RegionId { get; set; }

        public int Sequence { get; set; }

        public int Number { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [Required]
        [MaxLength(64)]
        public string Type { get; set; }

        [MaxLength(256)]
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
    }
}
