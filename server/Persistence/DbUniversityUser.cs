namespace advisor.Persistence {
    using HotChocolate;

    [GraphQLName("UniversityUser")]
    public class DbUniversityUser {
        public long UniversityId { get; set; }
        public long UserId { get; set; }

        public UniveristyMemberRole Role { get; set; }

        public DbUniversity University { get; set; }
        public DbUser User { get; set; }
    }
}
