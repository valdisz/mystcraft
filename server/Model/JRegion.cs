namespace atlantis.Model {
    public class JRegion {
        public string Terrain { get; set; }
        public JCoords Coords { get; set; }
        public string Province { get; set; }
        public JSettlement Settlement { get; set; }
        public JPopulation Population { get; set; }
        public int? Tax { get; set; }
        public double Wages { get; set; }
        public int? TotalWages { get; set; }
        public int? Entertainment { get; set; }
        public JTradableItem[] Wanted { get; set; } = new JTradableItem[0];
        public JTradableItem[] ForSale { get; set; } = new JTradableItem[0];
        public JItem[] Products { get; set; } = new JTradableItem[0];
        public JExit[] Exits { get; set; } = new JExit[0];
    }
}
