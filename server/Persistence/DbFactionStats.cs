namespace advisor.Persistence {
    using System.Collections.Generic;
    using HotChocolate;

    [GraphQLName("FactionStats")]
    public class DbFactionStats {
        [GraphQLIgnore]
        public long FactionId { get; set; }

        [GraphQLIgnore]
        public long TurnId { get; set; }

        [GraphQLIgnore]
        public long? RegionId { get; set; }

        public DbIncomeStats Income { get; set; }
        public List<DbItem> Production { get; set; } = new ();

        [GraphQLIgnore]
        public DbFaction Faction { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }
}
