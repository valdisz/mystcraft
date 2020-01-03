namespace atlantis.Persistence {
    using System.Collections.Generic;

    public class DbStructure {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public long RegionId { get; set; }

        public long Number { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }
        public DbRegion Region { get; set; }

        public List<DbUnit> Units { get; set; }
    }
}
