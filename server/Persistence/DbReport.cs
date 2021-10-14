namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Report")]
    public class DbReport {
        [Required]
        public int FactionNumber { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }



        [Required, MaxLength(100)]
        public string FactionName { get; set; }

        [Required]
        public string Source { get; set; }

        public string Json { get; set; }


        [GraphQLIgnore]
        public DbPlayer Player { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }
    }
}
