namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;
    using HotChocolate.Types;

    public class DbItem : AnItem {
        public DbItem() {

        }

        public DbItem(DbItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [Required]
        [MaxLength(Size.ITEM_CODE)]
        public string Code { get; set; }

        public int Amount { get; set; }
    }

    public class DbUnitItem : DbItem, InTurnContext {
        public DbUnitItem() {

        }

        public DbUnitItem(AnItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public int UnitNumber { get; set; }

        [DefaultValue(false)]
        public bool Illusion { get; set; }

        [DefaultValue(false)]
        public bool Unfinished { get; set; }

        public string Props { get; set; }
    }

    public class DbProductionItem : DbItem, InTurnContext {
        public DbProductionItem() {

        }

        public DbProductionItem(AnItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        [MaxLength(Size.REGION_ID)]
        public string RegionId { get; set; }
    }

    public class DbTradableItem : DbItem, InTurnContext {
        public DbTradableItem() {

        }

        public DbTradableItem(DbTradableItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
            this.Price = other.Price;
        }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        [MaxLength(Size.REGION_ID)]
        public string RegionId { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public Market Market { get; set; }
    }

    public class DbTreasuryItem : DbItem, InTurnContext {
        public DbTreasuryItem() {

        }

        public DbTreasuryItem(DbTreasuryItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
        }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        public int Rank { get; set; }

        public int Max { get; set; }

        public int Total { get; set; }

        [GraphQLIgnore]
        public DbPlayerTurn Turn { get; set; }
    }
}
