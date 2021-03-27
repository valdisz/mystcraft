namespace advisor.Model {
    using System.Collections.Generic;

    public class JUnit {
        public bool Own { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public JFaction Faction { get; set; }
        public string Description { get; set; }
        public bool OnGuard { get; set; }
        public List<string> Flags { get; set; } = new ();
        public List<JItem> Items { get; set; } = new ();
        public int? Weight { get; set; }
        public JCapacity Capacity { get; set; }
        public List<JSkill> Skills { get; set; } = new ();
        public List<JSkill> CanStudy { get; set; } = new ();
        public JItem ReadyItem { get; set; }
        public JSkill CombatSpell { get; set; }
    }
}
