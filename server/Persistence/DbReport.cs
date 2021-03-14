namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Report")]
    public class DbReport {
        [Key]
        public long Id { get; set; }


        [Required, GraphQLIgnore]
        public long PlayerId { get; set; }

        [Required, GraphQLIgnore]
        public long TurnId { get; set; }


        [Required]
        public int FactionNumber { get; set; }

        [Required, MaxLength(100)]
        public string FactionName { get; set; }

        [Required]
        public string Source { get; set; }

        [Required]
        public string Json { get; set; }


        [GraphQLIgnore]
        public DbPlayer Player { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }
    }
}
