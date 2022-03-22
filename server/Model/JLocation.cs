namespace advisor.Model {
    using HotChocolate;

    [GraphQLName("Location")]
    public class JLocation {
        public string Terrain { get; set; }
        public JCoords Coords { get; set; }
        public string Province { get; set; }
    }
}
