namespace advisor.Model
{
    public class JUnit {
        public bool Own { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public JFaction Faction { get; set; }
        public string Description { get; set; }
        public bool OnGuard { get; set; }
        public string[] Flags { get; set; } = new string[0];
        public JItem[] Items { get; set; } = new JItem[0];
        public int? Weight { get; set; }
        public JCapacity Capacity { get; set; }
        public JSkill[] Skills { get; set; } = new JSkill[0];
        public JSkill[] CanStudy { get; set; } = new JSkill[0];
        public JItem ReadyItem { get; set; }
        public JSkill CombatSpell { get; set; }
    }
}
