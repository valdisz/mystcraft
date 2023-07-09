namespace advisor.Model {
    using System.Collections.Generic;

    public class JFaction {
        public string Name { get; set; }
        public int Number { get; set; }
        public List<JFactionProp> Type { get; set; } = new ();
    }
}
