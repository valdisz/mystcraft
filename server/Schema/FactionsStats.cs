namespace advisor
{
    using System.Collections.Generic;
    using Persistence;

    public class FactionStats {
        public int FactionNumber { get; set; }
        public string FactionName { get; set; }
        public DbIncomeStats Income { get; set; }
        public List<Item> Production { get; set; }
    }
}
