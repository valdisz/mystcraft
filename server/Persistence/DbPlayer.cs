namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Player")]
    public class DbPlayer {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long UserId { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        public int? FactionNumber { get; set; }
        public string FactionName { get; set; }

        public int LastTurnNumber { get; set; }

        public string Password { get; set; }

        [GraphQLIgnore]
        public DbUser User { get;set; }

        [GraphQLIgnore]
        public DbGame Game { get;set; }

        [GraphQLIgnore]
        public List<DbTurn> Turns { get; set; } = new List<DbTurn>();

        [GraphQLIgnore]
        public List<DbReport> Reports { get; set; } = new List<DbReport>();

        [GraphQLIgnore]
        public DbUniversityMembership UniversityMembership { get; set; }
    }
}
