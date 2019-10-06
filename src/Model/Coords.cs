namespace atlantis
{
    using System.Linq;
    using Pidgin;
    using static Pidgin.Parser;
    using static Tokens;

    public class Coords {
        public Coords(int x, int y, string level) {
            X = x;
            Y = y;
            Level = level;
        }

        public int X { get; }
        public int Y { get; }
        public string Level { get; }
    }
}
