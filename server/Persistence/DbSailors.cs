namespace advisor.Persistence {
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    public class DbSailors {
        public int Current { get; set; }
        public int Required { get; set; }
    }
}
