namespace atlantis.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class DbGame {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        public int? PlayerFactionNumber { get; set; }
        public string EngineVersion { get; set; }
        public string RulesetName { get; set; }
        public string RulesetVersion { get; set; }
        public string Memory { get; set; }
        public string Password { get; set; }

        public List<DbTurn> Turns { get; set; }
    }
}
