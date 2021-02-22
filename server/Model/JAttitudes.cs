namespace atlantis.Model
{
    public class JAttitudes {
        public Attitude Default { get; set; }
        public JFaction[] Hostile { get; set; }
        public JFaction[] Unfriendly { get; set; }
        public JFaction[] Neutral { get; set; }
        public JFaction[] Friendly { get; set; }
        public JFaction[] Ally { get; set; }
    }
}
