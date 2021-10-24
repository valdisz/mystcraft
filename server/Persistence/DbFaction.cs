namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Faction")]
    public class DbFaction : InTurnContext {
        [Required]
        public int Number { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new List<DbEvent>();

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();

        [GraphQLIgnore]
        public List<DbStat> Stats { get; set; } = new ();
    }
}
