namespace advisor.Model
{
    public class JExit {
        public Direction Direction { get; set; }
        public string Terrain { get; set; }
        public JCoords Coords { get; set; }
        public string Province { get; set; }
        public JSettlement Settlement { get; set; }
    }
}
