namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Stat")]
    public class DbStat : InTurnContext {
        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        [Required]
        [MaxLength(14)]
        public string RegionId { get; set; }

        public DbIncomeStats Income { get; set; }

        public List<DbStatItem> Production { get; set; } = new ();

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }
}
