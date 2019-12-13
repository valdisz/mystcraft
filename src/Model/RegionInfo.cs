namespace atlantis {
    public class RegionInfo {
        public RegionInfo(Coords coords, string terrain, string province, Settlement settlement) {
            Coords = coords;
            Terrain = terrain;
            Province = province;
            Settlement = settlement;
        }

        public Coords Coords { get; }
        public string Terrain { get; }
        public string Province { get; }
        public Settlement Settlement { get; }
    }
}
