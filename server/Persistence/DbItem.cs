namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Item")]
    public interface AnItem {
        string Code { get; set; }

        int Amount { get; set; }
    }

    public class DbItem : AnItem {
        public DbItem() {

        }

        public DbItem(DbItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        public int Amount { get; set; }
    }

    public class DbUnitItem : AnItem, InTurnContext {
        public DbUnitItem() {

        }

        public DbUnitItem(AnItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        public int Amount { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int UnitNumber { get; set; }

        [GraphQLIgnore]
        public DbUnit Unit { get; set; }
    }

    public class DbProductionItem : AnItem, InTurnContext {
        public DbProductionItem() {

        }

        public DbProductionItem(AnItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        public int Amount { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        [MaxLength(14)]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }

    [GraphQLName("TradableItem")]
    public class DbTradableItem : AnItem, InTurnContext  {
        public DbTradableItem() {

        }

        public DbTradableItem(DbTradableItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
            this.Price = other.Price;
        }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        public int Amount { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public Market Market { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        [MaxLength(14)]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }
    }

    public class DbStatItem : AnItem, InFactionContext {
        public DbStatItem() {

        }

        public DbStatItem(AnItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [Required]
        [MaxLength(8)]
        public string Code { get; set; }

        public int Amount { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public int FactionNumber { get; set; }

        [GraphQLIgnore]
        [MaxLength(14)]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        [GraphQLIgnore]
        public DbFaction Faction { get; set; }

        [GraphQLIgnore]
        public DbStat Stat { get; set; }
    }
}
