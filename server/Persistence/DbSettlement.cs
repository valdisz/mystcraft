namespace advisor.Persistence {
    using advisor.Model;
    using Microsoft.EntityFrameworkCore;
    using HotChocolate;
    using System.ComponentModel.DataAnnotations;

    [Owned]
    public class DbSettlement {
        [MaxLength(advisor.Persistence.Size.SETTLEMENT)]
        public string Name { get; set; }

        public SettlementSize Size { get; set; }
    }
}
