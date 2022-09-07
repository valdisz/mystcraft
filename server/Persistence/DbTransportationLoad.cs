namespace advisor.Persistence {
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    public class DbTransportationLoad {
        public int Used { get; set; }
        public int Max { get; set; }
    }
}
