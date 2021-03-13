namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    [GraphQLName("TradableItem")]
    public class DbTradableItem  {
        public DbTradableItem() {

        }

        public DbTradableItem(DbTradableItem other) {
            this.Code = other.Code;
            this.Amount = other.Amount;
            this.Price = other.Price;
        }

        [Required]
        public string Code { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public int Price { get; set; }
    }
}
