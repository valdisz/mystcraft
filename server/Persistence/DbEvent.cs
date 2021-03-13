namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;

    [GraphQLName("Event")]
    public class DbEvent {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long TurnId { get; set; }

        [GraphQLIgnore]
        public long FactionId { get; set; }

        [Required]
        public EventType Type { get; set; }

        [Required]
        public string Message { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        public DbFaction Faction { get; set; }
    }
}
