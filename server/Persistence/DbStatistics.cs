namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public class DbStatistics : InTurnContext {
        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        [Required]
        [MaxLength(14)]
        public string RegionId { get; set; }

        public DbIncome Income { get; set; } = new ();
        public DbExpenses Expenses { get; set; } = new ();

        public List<DbStatisticsItem> Items { get; set; } = new ();

        [GraphQLIgnore]
        public DbPlayerTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }
}
