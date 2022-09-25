namespace advisor.Schema {
    using System.Collections.Generic;
    using Persistence;

    public class Statistics {
        public DbIncome Income { get; set; }
        public List<Item> Production { get; set; }
    }
}
