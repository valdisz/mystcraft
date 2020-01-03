namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DbFaction {
        [Key]
        public long Id { get; set; }

        public long GameId { get; set; }
        public long TurnId { get; set; }

        public bool Own { get; set; }
        public int Number { get; set; }


        [Required]
        public string Name { get; set; }

        [Required]
        public string Json { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }

        public List<DbEvent> Events { get; set; }
    }
}
