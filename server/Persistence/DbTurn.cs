namespace atlantis.Persistence {
    using System.Collections.Generic;

    public class DbTurn {
        public long Id { get; set; }
        public long GameId { get; set; }

        public int Number { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        public DbGame Game { get; set; }

        public List<DbFaction> Factions { get; set; }
        public List<DbRegion> Regions { get; set; }
        public List<DbUnit> Units { get; set; }
        public List<DbStructure> Structures { get; set; }
    }
}
