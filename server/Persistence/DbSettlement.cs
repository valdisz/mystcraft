namespace advisor.Persistence {
    using advisor.Model;
    using Microsoft.EntityFrameworkCore;
    using HotChocolate;
    using System.ComponentModel.DataAnnotations;

    [Owned]
    public class DbSettlement {
        [MaxLength(256)]
        public string Name { get; set; }

        public SettlementSize Size { get; set; }
    }
}
