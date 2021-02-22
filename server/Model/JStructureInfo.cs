namespace atlantis.Model
{
    public class JStructureInfo {
        public int Number { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public JFleetContent[] Contents { get; set; }
        public string[] Flags { get; set; }
        public Direction[] SailDirections { get; set; }
        public int? Speed { get; set; }
        public int? Needs { get; set; }
        public JTransportationLoad Load { get; set; }
        public JSailors Sailors { get; set; }
    }
}
