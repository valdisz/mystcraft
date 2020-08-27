namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Turn")]
    public class DbTurn {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        public int Number { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        // public string Memory { get; set; }

        [GraphQLIgnore]
        public DbGame Game { get; set; }

        [GraphQLIgnore]
        public List<DbReport> Reports { get; set; } = new List<DbReport>();

        [GraphQLIgnore]
        public List<DbRegion> Regions { get; set; } = new List<DbRegion>();

        // public List<DbFaction> Factions { get; set; }
        // public List<DbEvent> Events { get; set; }
        // public List<DbUnit> Units { get; set; }
        // public List<DbStructure> Structures { get; set; }
    }
}
