namespace atlantis.Persistence {
    using System.ComponentModel.DataAnnotations;
    using atlantis.Model;

    public class DbEvent {
        [Key]
        public long Id { get; set; }

        public long TurnId { get; set; }
        public long FactionId { get; set; }

        [Required]
        public EventType Type { get; set; }

        [Required]
        public string Message { get; set; }

        public DbTurn Turn { get; set; }
        public DbFaction Faction { get; set; }
    }
}
