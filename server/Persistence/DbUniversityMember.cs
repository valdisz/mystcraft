namespace advisor.Persistence {
    using HotChocolate;

    [GraphQLName("UniversityMember")]
    public class DbUniversityMembership {
        [GraphQLIgnore]
        public long UniversityId { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public UniveristyMemberRole Role { get; set; }

        [GraphQLIgnore]
        public DbUniversity University { get; set; }

        public DbPlayer Player { get; set; }
    }
}
