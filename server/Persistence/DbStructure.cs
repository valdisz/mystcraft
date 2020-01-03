namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DbStructure {
        [Key]
        public long Id { get; set; }

        public long GameId { get; set; }
        public long TurnId { get; set; }
        public long RegionId { get; set; }

        public int Sequence { get; set; }
        public int Number { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public string Json { get; set; }

        [Required]
        public string Memory { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }
        public DbRegion Region { get; set; }

        public List<DbUnit> Units { get; set; }
    }
}
