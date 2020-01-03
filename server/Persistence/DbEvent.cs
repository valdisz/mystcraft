using System.ComponentModel.DataAnnotations;

namespace atlantis.Persistence
{
    public class DbEvent {
        [Key]
        public long Id { get; set; }
        
        public long GameId { get; set; }
        public long TurnId { get; set; }
        public long FactionId { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Json { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }
        public DbFaction Faction { get; set; }
    }
}
