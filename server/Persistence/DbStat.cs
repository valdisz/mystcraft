namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Stat")]
    public class DbStat {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long TurnId { get; set; }

        [GraphQLIgnore]
        public long FactionId { get; set; }

        [GraphQLIgnore]
        public long? RegionId { get; set; }

        public DbIncomeStats Income { get; set; }
        public List<DbItem> Production { get; set; } = new ();

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbFaction Faction { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }
}
