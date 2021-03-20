namespace advisor.Model
{
    public class JReport {
        public JFaction Faction { get; set; }
        public JDate Date { get; set; }
        public JGameEngine Engine { get; set; }
        public JFactionStatus[] FactionStatus { get; set; } = new JFactionStatus[0];
        public string[] Errors { get; set; } = new string[0];
        public JEvent[] Events { get; set; } = new JEvent[0];
        public JSkillReport[] SkillReports { get; set; } = new JSkillReport[0];
        public JItemReport[] itemReports { get; set; } = new JItemReport[0];
        public JAttitudes Attitudes { get; set; }
        public int? UnclaimedSilver { get; set; }
        public JRegion[] Regions { get; set; } = new JRegion[0];
        public JOrdersTemplate OrdersTemplate { get; set; }
    }
}
