namespace advisor.Model {
    using System.Collections.Generic;

    public class JStructureInfo {
        public int Number { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<JFleetContent> Contents { get; set; } = new ();
        public List<string> Flags { get; set; } = new ();
        public List<Direction> SailDirections { get; set; } = new ();
        public int? Speed { get; set; }
        public int? Needs { get; set; }
        public JTransportationLoad Load { get; set; }
        public JSailors Sailors { get; set; }
    }
}
