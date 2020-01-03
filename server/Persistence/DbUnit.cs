namespace atlantis.Persistence {
    public class DbUnit {
        public long Id { get; set; }
        public long GameId { get; set; }
        public long TurnId { get; set; }

        public long RegionId { get; set; }
        public long? StrcutureId { get; set; }

        public long Number { get; set; }

        public DbGame Game { get; set; }
        public DbTurn Turn { get; set; }
        public DbRegion Region { get; set; }
        public DbStructure Structure { get; set; }
    }
}
