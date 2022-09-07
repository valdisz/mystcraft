namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

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
        public DbPlayerTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }
}
