namespace atlantis.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("UserGame")]
    public class DbUserGame {
        [Key]
        public long Id { get; set; }

        public long UserId { get; set; }

        public long GameId { get; set; }

        public int? PlayerFactionNumber { get; set; }
        public string PlayerFactionName { get; set; }

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
    }
}
