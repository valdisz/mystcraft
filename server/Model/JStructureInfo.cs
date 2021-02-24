namespace atlantis.Model
{
    public class JStructureInfo {
        public int Number { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public JFleetContent[] Contents { get; set; } = new JFleetContent[0];
        public string[] Flags { get; set; } = new string[0];
        public Direction[] SailDirections { get; set; } = new Direction[0];
        public int? Speed { get; set; }
        public int? Needs { get; set; }
        public JTransportationLoad Load { get; set; }
        public JSailors Sailors { get; set; }
    }
}
