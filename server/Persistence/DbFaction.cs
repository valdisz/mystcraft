namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DbFaction {
        [Key]
        public long Id { get; set; }

        public long TurnId { get; set; }


        [Required]
        public int Number { get; set; }

        [Required]
        public string Name { get; set; }

        public DbTurn Turn { get; set; }

        public List<DbEvent> Events { get; set; } = new List<DbEvent>();
    }
}
