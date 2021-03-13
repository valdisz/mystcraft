namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
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
}
