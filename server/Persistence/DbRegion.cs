namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DbRegion {
        [Key]
        public long Id { get; set; }

        public long GameId { get; set; }
        public long TurnId { get; set; }

        public string EmpheralId => $"{X} {Y} {Z}";

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public int UpdatedAtTurn { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        public string Terrain { get; set; }

        [Required]
        public string Json { get; set; }

        public string Memory { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }

        public List<DbUnit> Units { get; set; }
        public List<DbStructure> Structures { get; set; }
    }
}
