namespace atlantis.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    [GraphQLName("Item")]
    public class DbItem {
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Count { get; set; }
    }

    [Owned]
    [GraphQLName("TradableItem")]
    public class DbTradableItem : DbItem {
        [Required]
        public int Price { get; set; }
    }
}
