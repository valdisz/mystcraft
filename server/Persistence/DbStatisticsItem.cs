namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;

    public enum StatisticsCategory {
        Produced,
        Bought,
        Sold,
        Consumed,
    }

    public class DbStatisticsItem : AnItem, InTurnContext {
        public DbStatisticsItem() {

        }

        public DbStatisticsItem(AnItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }


        [GraphQLIgnore]
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        [Required]
        [MaxLength(14)]
        public string RegionId { get; set; }

        public StatisticsCategory Category { get; set; }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        public int Amount { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        [GraphQLIgnore]
        public DbStatistics Statistics { get; set; }
    }
}
