namespace atlantis.Persistence {
    using System.Collections.Generic;

    public class DbFaction {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public int Number { get; set; }
        public string Name { get; set; }

        public string Json { get; set; }
        public string Memory { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }

        public List<DbEvent> Events { get; set; }
    }
}
