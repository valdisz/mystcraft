namespace advisor.Persistence {
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    [GraphQLName("Sailors")]
    public class DbSailors {
        public int Current { get; set; }
        public int Required { get; set; }
    }
}
