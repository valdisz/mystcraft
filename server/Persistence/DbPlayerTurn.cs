namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using HotChocolate;

    public class DbPlayerTurn : InTurnContext {
        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        [Required]
        [MaxLength(128)]
        public string Name { get; set; }


        public bool Ready { get; set; }
        public bool OrdersSubmitted { get; set; }
        public bool TimesSubmitted { get; set; }


        [GraphQLIgnore]
        public DbGame Game { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbPlayer Player { get; set; }

        [GraphQLIgnore]
        public List<DbAditionalReport> Reports { get; set; } = new List<DbAditionalReport>();

        [GraphQLIgnore]
        public List<DbRegion> Regions { get; set; } = new List<DbRegion>();

        [GraphQLIgnore]
        public List<DbExit> Exits { get; set; } = new List<DbExit>();

        [GraphQLIgnore]
        public List<DbMarketItem> Markets { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbProductionItem> Production { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbFaction> Factions { get; set; } = new List<DbFaction>();

        [GraphQLIgnore]
        public List<DbAttitude> Attitudes { get; set; } = new List<DbAttitude>();

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new List<DbEvent>();

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();

        [GraphQLIgnore]
        public List<DbUnitItem> Items { get; set; } = new List<DbUnitItem>();

        [GraphQLIgnore]
        public List<DbStructure> Structures { get; set; } = new List<DbStructure>();

        [GraphQLIgnore]
        public List<DbStudyPlan> Plans { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbStat> Stats { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbBattle> Battles { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbOrders> Orders { get; set; } = new ();
    }
}
