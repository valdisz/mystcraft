namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;

    [GraphQLName("Event")]
    public class DbEvent : InFactionContext {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public int FactionNumber { get; set; }

        [GraphQLName("regionCode")]
        [MaxLength(14)]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public int? UnitNumber { get; set; }

        [MaxLength(128)]
        public string UnitName { get; set; }

        [GraphQLIgnore]
        public int? MissingUnitNumber { get; set; }

        [Required]
        public EventType Type { get; set; }

        [Required]
        public EventCategory Category { get; set; } = EventCategory.Unknown;

        [Required]
        public string Message { get; set; }

        public int? Amount { get; set; }

        [MaxLength(8)]
        public string ItemCode { get; set; }

        [MaxLength(256)]
        public string ItemName { get; set; }

        public int? ItemPrice { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbFaction Faction { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        [GraphQLIgnore]
        public DbUnit Unit { get; set; }
    }
}
