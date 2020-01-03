namespace atlantis.Persistence {
    public class DbUnit {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public long RegionId { get; set; }
        public long? StrcutureId { get; set; }

        public bool Own { get; set; }
        public int Sequence { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public int? FactionNumber { get; set; }
        public string Json { get; set; }
        public string Memory { get; set; }
        public string Orders { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }
        public DbRegion Region { get; set; }
        public DbStructure Structure { get; set; }
    }
}
