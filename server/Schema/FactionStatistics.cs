namespace advisor.Schema {
    using System.Collections.Generic;
    using Persistence;

    public class Statistics {
        public DbIncomeStats Income { get; set; }
        public List<Item> Production { get; set; }
    }
}
