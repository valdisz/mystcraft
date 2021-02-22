namespace atlantis.Persistence
{
    using atlantis.Model;
    using Microsoft.EntityFrameworkCore;
    using HotChocolate;

    [Owned]
    [GraphQLName("Settlement")]
    public class DbSettlement {
        public string Name { get; set; }
        public SettlementSize Size { get; set; }
    }
}
