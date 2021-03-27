namespace advisor.Model {
    using System.Collections.Generic;

    public class JRegion {
        public string Terrain { get; set; }
        public JCoords Coords { get; set; }
        public string Province { get; set; }
        public JSettlement Settlement { get; set; }
        public JPopulation Population { get; set; }
        public int Tax { get; set; } = 0;
        public double Wages { get; set; } = 0;
        public int TotalWages { get; set; } = 0;
        public int Entertainment { get; set; } = 0;
        public List<JTradableItem> Wanted { get; set; } = new ();
        public List<JTradableItem> ForSale { get; set; } = new ();
        public List<JItem> Products { get; set; } = new ();
        public List<JExit> Exits { get; set; } = new ();
        public List<JUnit> Units { get; set; } = new ();
        public List<JStructure> Structures {get; set; } = new ();
    }
}
