namespace advisor.Persistence
{
    using System.Collections.Generic;

    public class DbGameTurn {
        public int Number { get; set; }
        public long GameId { get; set; }

        public byte[] PlayerData { get; set; }
        public byte[] GameData { get; set; }

        public DbGame Game { get; set; }
        public List<DbGameArticle> Articles { get; set; } = new ();
    }
}
