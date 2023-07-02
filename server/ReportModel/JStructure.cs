namespace advisor.Model {
    using System.Collections.Generic;

    public class JStructure {
        public JStructureInfo Structure { get; set; }
        public List<JUnit> Units {get; set; } = new ();
    }
}
