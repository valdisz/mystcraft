namespace advisor.Persistence
{
    using System.Collections.Generic;
    using HotChocolate;

    public class DbGameTurn : InGameContext {
        public int Number { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        [GraphQLIgnore]
        public byte[] PlayerData { get; set; }

        [GraphQLIgnore]
        public byte[] GameData { get; set; }

        public bool IsRemote { get; set; }

        public DbGame Game { get; set; }
        public List<DbArticle> Articles { get; set; } = new ();
        public List<DbGameReport> Reports { get; set; } = new ();
    }
}
