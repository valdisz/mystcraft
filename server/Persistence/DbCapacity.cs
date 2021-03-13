namespace advisor.Persistence {
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    [GraphQLName("Capacity")]
    public class DbCapacity {
        public int Flying { get; set; }
        public int Riding { get; set; }
        public int Walking { get; set; }
        public int Swimming { get; set; }
    }
}
