namespace advisor.Persistence {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public class DbGame {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(Size.LABEL)]
        public string Name { get; set; }

        [MaxLength(Size.TYPE)]
        public GameType Type { get; set; }

        [MaxLength(Size.TYPE)]
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


        [GraphQLIgnore]
        public List<DbPlayer> Players { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbRegistration> Registrations { get; set; } = new ();

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
