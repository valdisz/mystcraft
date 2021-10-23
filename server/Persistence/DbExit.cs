namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [GraphQLName("Exit")]
    public class DbExit : InTurnContext {
        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        [Required]
        public string OriginRegionId { get; set; }

        [GraphQLIgnore]
        [Required]
        public string TargetRegionId { get; set; }

        [Required]
        public Direction Direction { get; set; }

        public DbRegion Origin { get; set; }

        public DbRegion Target { get; set; }
    }
}
