namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Game")]
    public class DbGame {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public GameType Type { get; set; }

        [GraphQLIgnore]
        public byte[] Engine { get; set; }

        [Required]
        public GameOptions Options { get; set; }

        [Required]
        public string Ruleset { get; set; }

        [Required]
        [MaxLength(128)]
        public string EngineVersion { get; set; }

        [Required]
        [MaxLength(128)]
        public string RulesetName { get; set; }

        [Required]
        [MaxLength(128)]
        public string RulesetVersion { get; set; }

        [GraphQLIgnore]
        public List<DbPlayer> Players { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbAlliance> Alliances { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbGameTurn> Turns { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbGameArticle> Articles { get; set; } = new ();
    }
}
