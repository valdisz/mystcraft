namespace advisor.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

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

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        [Required]
        [MaxLength(256)]
        public string Label { get; set; }

        [Required]
        [MaxLength(256)]
        public string Province { get; set; }

        [Required]
        [MaxLength(256)]
        public string Terrain { get; set; }

        public DbSettlement Settlement { get; set; }

        [GraphQLIgnore]
        public DbRegion Origin { get; set; }

        [GraphQLIgnore]
        public DbRegion Target { get; set; }

        [GraphQLIgnore]
        public DbPlayerTurn Turn { get; set; }
    }
}
