namespace advisor.Persistence {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public class DbPlayerTurn : InPlayerContext {
        public static string CreateId(long playerId, int turnNumber) => $"{playerId}:{turnNumber}";
        public static string CreateId(DbPlayerTurn player) => CreateId(player.PlayerId, player.TurnNumber);
        public static (long playerId, int turnNumber) ParseId(string id) {
            var values = id.Split(":");
            return (
                long.Parse(values[0]),
                int.Parse(values[1])
            );
        }


        public string Id => CreateId(this);

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public int TurnNumber { get; set; }


        [Required]
        [MaxLength(128)]
        public string Name { get; set; }
        public DateTimeOffset? ReadyAt { get; set; }
        public DateTimeOffset? OrdersSubmittedAt { get; set; }
        public DateTimeOffset? TimesSubmittedAt { get; set; }

        public bool IsReady => ReadyAt.HasValue;
        public bool IsOrdersSubmitted => OrdersSubmittedAt.HasValue;
        public bool IsTimesSubmitted => TimesSubmittedAt.HasValue;


        [GraphQLIgnore]
        public DbPlayer Player { get; set; }

        [GraphQLIgnore]
        public List<DbAditionalReport> Reports { get; set; } = new List<DbAditionalReport>();

        [GraphQLIgnore]
        public List<DbRegion> Regions { get; set; } = new List<DbRegion>();

        [GraphQLIgnore]
        public List<DbExit> Exits { get; set; } = new List<DbExit>();

        [GraphQLIgnore]
        public List<DbMarketItem> Markets { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbProductionItem> Production { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbFaction> Factions { get; set; } = new List<DbFaction>();

        [GraphQLIgnore]
        public List<DbAttitude> Attitudes { get; set; } = new List<DbAttitude>();

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new List<DbEvent>();

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();

        [GraphQLIgnore]
        public List<DbUnitItem> Items { get; set; } = new List<DbUnitItem>();

        [GraphQLIgnore]
        public List<DbStructure> Structures { get; set; } = new List<DbStructure>();

        [GraphQLIgnore]
        public List<DbStudyPlan> Plans { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbStat> Stats { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbBattle> Battles { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbOrders> Orders { get; set; } = new ();
    }
}
