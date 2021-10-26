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
        [MaxLength(14)]
        public string OriginRegionId { get; set; }

        [GraphQLName("targetRegion")]
        [Required]
        [MaxLength(14)]
        public string TargetRegionId { get; set; }

        [Required]
        public Direction Direction { get; set; }

        [GraphQLIgnore]
        public DbRegion Origin { get; set; }

        [GraphQLIgnore]
        public DbRegion Target { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }
    }
}
