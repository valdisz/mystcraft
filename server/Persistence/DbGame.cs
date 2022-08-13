namespace advisor.Persistence {
    using System;
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

        public bool Finished { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [GraphQLIgnore]
        public long? EngineId { get; set; }

        [Required]
        public GameOptions Options { get; set; }

        [Required]
        public string Ruleset { get; set; }


        public int? LastTurnNumber { get; set; }

        public int? NextTurnNumber { get; set; }

        [GraphQLIgnore]
        public DbGameTurn LastTurn { get; set; }

        [GraphQLIgnore]
        public DbGameTurn NextTurn { get; set; }


        [GraphQLIgnore]
        public List<DbPlayer> Players { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbAlliance> Alliances { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbGameTurn> Turns { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbGameArticle> Articles { get; set; } = new ();

        [GraphQLIgnore]
        public DbGameEngine Engine { get; set; }
    }
}
