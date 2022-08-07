namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Article")]
    public class DbGameArticle {
        public long Id { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        public long? PlayerId { get; set; }

        [Required, MaxLength(32)]
        public string Type { get; set; }

        [Required]
        public string Text { get; set; }

        [GraphQLIgnore]
        public DbGame Game { get; set; }

        [GraphQLIgnore]
        public DbGameTurn Turn { get; set; }
    }
}
