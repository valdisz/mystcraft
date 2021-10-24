namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Turn")]
    public class DbTurn : InPlayerContext {
        public int Number { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [GraphQLIgnore]
        public DbPlayer Player { get; set; }

        [GraphQLIgnore]
        public List<DbReport> Reports { get; set; } = new List<DbReport>();

        [GraphQLIgnore]
        public List<DbRegion> Regions { get; set; } = new List<DbRegion>();

        [GraphQLIgnore]
        public List<DbTradableItem> Markets { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbProductionItem> Production { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbFaction> Factions { get; set; } = new List<DbFaction>();

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
    }
}
