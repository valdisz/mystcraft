namespace advisor
{
    using System.Collections.Generic;
    using Persistence;

    public class FactionsStats {
        public DbIncomeStats Income { get; set; }
        public List<DbItem> Production { get; set; }
    }
}
