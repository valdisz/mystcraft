namespace atlantis.Persistence {
    using System.Collections.Generic;

    public class DbGame {
        public long Id { get; set; }
        public string Name { get; set; }

        public List<DbTurn> Turns { get; set; }
    }
}
