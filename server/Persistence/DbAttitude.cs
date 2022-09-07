namespace advisor.Persistence {
    using advisor.Model;
    using HotChocolate;


    public class DbAttitude : InFactionContext {
        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public int FactionNumber { get; set; }

        [GraphQLName("factionNumber")]
        public int TargetFactionNumber { get; set; }

        public Stance Stance { get; set; }

        [GraphQLIgnore]
        public DbPlayerTurn Turn{ get; set; }

        [GraphQLIgnore]
        public DbFaction Faction { get; set; }
    }
}
