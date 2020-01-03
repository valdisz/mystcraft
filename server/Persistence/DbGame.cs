namespace atlantis.Persistence {
    using System.Collections.Generic;

    public class DbGame {
        public long Id { get; set; }
        public string Name { get; set; }

        public int? PlayerFactionNumber { get; set; }
        public string EngineVersion { get; set; }
        public string RulesetVersion { get; set; }

        public List<DbTurn> Turns { get; set; }
    }
}
