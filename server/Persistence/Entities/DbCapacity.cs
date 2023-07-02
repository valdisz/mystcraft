namespace advisor.Persistence {
    using Microsoft.EntityFrameworkCore;

    [Owned]
    public class DbCapacity {
        public int Flying { get; set; }
        public int Riding { get; set; }
        public int Walking { get; set; }
        public int Swimming { get; set; }
    }
}
