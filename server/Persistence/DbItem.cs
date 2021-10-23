namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Item")]
    public abstract class DbItem {
        public DbItem() {

        }

        public DbItem(DbItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [Required]
        public string Code { get; set; }

        public int? Amount { get; set; }
    }

    public class DbUnitItem : DbItem, InTurnContext {
        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int UnitNumber { get; set; }

        [GraphQLIgnore]
        public DbUnit Unit { get; set; }
    }

    public class DbProductionItem : DbItem, InTurnContext {
        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }

    [GraphQLName("TradableItem")]
    public class DbTradableItem : DbItem, InTurnContext  {
        public DbTradableItem() {

        }

        public DbTradableItem(DbTradableItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
            this.Price = other.Price;
        }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public Market Market { get; set; }
    }

    public class DbStatItem : DbItem, InFactionContext {
        public DbStatItem() {

        }

        public DbStatItem(DbItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public int FactionNumber { get; set; }

        [GraphQLIgnore]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        [GraphQLIgnore]
        public DbFaction Faction { get; set; }

        [GraphQLIgnore]
        public DbStat Stat { get; set; }
    }
}
