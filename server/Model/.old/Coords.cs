namespace atlantis
{
    public class Coords {
        public Coords(int x, int y, int z, string level) {
            X = x;
            Y = y;
            Z = z;
            Level = level;
        }

        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public string Level { get; }
    }
}
