namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Game")]
    public class DbGame {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        public GameType Type { get; set; }

        [GraphQLIgnore]
        public string Options { get; set; }

        public string Ruleset { get; set; }

        public string EngineVersion { get; set; }
        public string RulesetName { get; set; }
        public string RulesetVersion { get; set; }

        [GraphQLIgnore]
        public List<DbPlayer> Players { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbUniversity> Universities { get; set; } = new ();
    }
}
