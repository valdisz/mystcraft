namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public class DbPlayer : InGameContext {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long? UserId { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        public int? Number { get; set; }

        [MaxLength(128)]
        public string Name { get; set; }

        public int LastTurnNumber { get; set; }

        [MaxLength(64)]
        public string Password { get; set; }

        public bool IsQuit { get; set; }

        [GraphQLIgnore]
        public DbUser User { get;set; }

        [GraphQLIgnore]
        public DbGame Game { get;set; }

        [GraphQLIgnore]
        public List<DbPlayerTurn> Turns { get; set; } = new List<DbPlayerTurn>();

        [GraphQLIgnore]
        public List<DbReport> Reports { get; set; } = new List<DbReport>();

        [GraphQLIgnore]
        public List<DbAllianceMember> AllianceMembererships { get; set; } = new ();
    }
}
