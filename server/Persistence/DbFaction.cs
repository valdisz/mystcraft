namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Faction")]
    public class DbFaction {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long TurnId { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        public string Name { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new List<DbEvent>();

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();
    }
}
