namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Item")]
    public class DbItem {
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

    public class DbUnitItem : DbItem {
        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int UnitNumber { get; set; }

        [GraphQLIgnore]
        public DbUnit Unit { get; set; }
    }
}
