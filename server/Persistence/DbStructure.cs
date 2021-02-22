namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Structure")]
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

        public string Memory { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }
        public DbRegion Region { get; set; }

        public List<DbUnit> Units { get; set; }

        public static string GetEmpheralId(int x, int y, int z, int number, string type) => $"{x} {y} {z} {number} {type}";
    }
}
