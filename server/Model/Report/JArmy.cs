namespace advisor.Model {
    using HotChocolate;

    [GraphQLName("Participant")]
    public class JParticipant {
        public string Name { get; set; }
        public int Number { get; set; }
    }
}
