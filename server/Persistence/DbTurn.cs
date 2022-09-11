namespace advisor.Persistence
{
    using System.Collections.Generic;
    using HotChocolate;

    public class DbTurn : InGameContext {

        [GraphQLIgnore]
        public long GameId { get; set; }

        public int Number { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        [GraphQLIgnore]
        public byte[] PlayerData { get; set; }

        [GraphQLIgnore]
        public byte[] GameData { get; set; }

        public DbGame Game { get; set; }
        public List<DbArticle> Articles { get; set; } = new ();
        public List<DbReport> Reports { get; set; } = new ();
    }
}
