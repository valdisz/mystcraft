namespace atlantis.Persistence {
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Region")]
    public class DbRegion {
        [Key]
        public long Id { get; set; }


        [GraphQLIgnore]
        public long TurnId { get; set; }

        [GraphQLIgnore]
        public string UID => GetUID(X, Y, Z);

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

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        // public List<DbUnit> Units { get; set; }
        // public List<DbStructure> Structures { get; set; }

        public static string GetUID(int x, int y, int z) => $"{x} {y} {z}";
    }
}
