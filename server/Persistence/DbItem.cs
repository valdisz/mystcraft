namespace atlantis.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    [GraphQLName("Item")]
    public class DbItem {
        [GraphQLIgnore]
        [Required]
        public long TurnId { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public int Amount { get; set; }
    }

    [Owned]
    [GraphQLName("TradableItem")]
    public class DbTradableItem  {
        [GraphQLIgnore]
        [Required]
        public long TurnId { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public int Amount { get; set; }

        [Required]
        public int Price { get; set; }
    }
}
