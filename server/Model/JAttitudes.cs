namespace advisor.Model {
    using System.Collections.Generic;

    public class JAttitudes {
        public Stance Default { get; set; }
        public List<JFaction> Hostile { get; set; } = new ();
        public List<JFaction> Unfriendly { get; set; } = new ();
        public List<JFaction> Neutral { get; set; } = new ();
        public List<JFaction> Friendly { get; set; } = new ();
        public List<JFaction> Ally { get; set; } = new ();
    }
}
