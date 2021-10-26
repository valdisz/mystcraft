namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using HotChocolate;

    [GraphQLName("Region")]
    public class DbRegion : InTurnContext {
        [MaxLength(14)]
        [Required]
        public string Id { get; set; }

        public static string MakeId(int x, int y, int z) => $"{x},{y},{z}";

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        // Full region report was obtained
        public bool Explored { get; set; }

        // Last turn OWN unit was in the region
        public int? LastVisitedAt { get; set; }

        [Required]
        [MaxLength(256)]
        public string Label { get; set; }

        [Required]
        [MaxLength(256)]
        public string Province { get; set; }

        [Required]
        [MaxLength(256)]
        public string Terrain { get; set; }

        public DbSettlement Settlement { get; set; }

        [Required]
        public int Population { get; set; }

        [MaxLength(256)]
        public string Race { get; set; }

        [Required]
        public int Entertainment { get; set; }

        [Required]
        public int Tax { get; set; }

        [Required]
        public double Wages { get; set; }

        [Required]
        public int TotalWages { get; set; }

        [GraphQLIgnore]
        public List<DbMarketItem> Markets { get; set; } = new List<DbMarketItem>();

        public IEnumerable<DbMarketItem> ForSale => Markets.Where(x => x.Market == Persistence.Market.FOR_SALE);
        public IEnumerable<DbMarketItem> Wanted => Markets.Where(x => x.Market == Persistence.Market.WANTED);

        public List<DbProductionItem> Produces { get; set; } = new List<DbProductionItem>();

        public List<DbExit> Exits { get; set; } = new List<DbExit>();

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();

        [GraphQLIgnore]
        public List<DbStructure> Structures { get; set; } = new List<DbStructure>();

        [GraphQLIgnore]
        public List<DbStat> Stats { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new ();
    }
}
