namespace advisor.Persistence {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public class DbGame {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public GameType Type { get; set; }

        public GameStatus Status { get; set; }

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

        // The last processed or imported turn
        [GraphQLIgnore]
        public DbTurn LastTurn { get; set; }

        // The pending turn, not yet processed
        [GraphQLIgnore]
        public DbTurn NextTurn { get; set; }


        [GraphQLIgnore]
        public List<DbPlayer> Players { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbAlliance> Alliances { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbTurn> Turns { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbArticle> Articles { get; set; } = new ();

        [GraphQLIgnore]
        public DbGameEngine Engine { get; set; }
    }
}
