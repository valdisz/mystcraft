namespace atlantis.Model
{
    public class JUnit {
        public bool Own { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public JFaction Faction { get; set; }
        public string Description { get; set; }
        public bool OnGuard { get; set; }
        public string[] Flags { get; set; }
        public JItem[] Items { get; set; }
        public int? Weight { get; set; }
        public JCapacity Capacity { get; set; }
        public JSkill[] Skills { get; set; }
        public JSkill[] CanStudy { get; set; }
        public JItem ReadyItem { get; set; }
        public JSkill CombatSpell { get; set; }
    }
}
