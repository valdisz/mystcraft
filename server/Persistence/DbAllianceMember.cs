namespace advisor.Persistence
{
    using HotChocolate;

    public class DbAllianceMember : InPlayerContext {
        [GraphQLIgnore]
        public long AllianceId { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public bool ShareMap { get; set; }
        public bool TeachMages { get; set; }
        public bool Owner { get; set; }
        public bool CanInvite { get; set; }

        [GraphQLIgnore]
        public DbAlliance Alliance { get; set; }

        [GraphQLIgnore]
        public DbPlayer Player { get; set; }
    }
}
