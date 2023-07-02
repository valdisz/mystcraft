namespace advisor.Model {
    using HotChocolate;

    [GraphQLName("Coords")]
    public class JCoords {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string Label { get; set; }

        public override string ToString() => $"{X},{Y},{Z} {Label}";
    }
}
