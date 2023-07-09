namespace advisor.Model {
    using HotChocolate;

    [GraphQLName("BattleRound")]
    public class JBattleRound {
        public string Log { get; set; }
        public string Statistics { get; set; }
    }
}
