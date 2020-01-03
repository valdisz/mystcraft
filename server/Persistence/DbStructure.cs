namespace atlantis.Persistence {
    using System.Collections.Generic;

    public class DbStructure {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public long RegionId { get; set; }

        public int Sequence { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Json { get; set; }
        public string Memory { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }
        public DbRegion Region { get; set; }

        public List<DbUnit> Units { get; set; }
    }
}
