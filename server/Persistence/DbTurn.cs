namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using HotChocolate;

    using TurnId = System.ValueTuple<long, int>;

    [GraphQLName("Turn")]
    public class DbTurn : InPlayerContext {
        [GraphQLIgnore]
        [NotMapped]
        public string CompsiteId => MakeId(this);

        public static string MakeId(DbTurn turn) => MakeId(turn.PlayerId, turn.Number);
        public static string MakeId(long playerId, int turnNumber) => $"{playerId}/{turnNumber}";
        public static TurnId ParseId(string id) {
            var segments = id.Split("/");
            return (
                long.Parse(segments[0]),
                int.Parse(segments[1])
            );
        }

        public static IQueryable<DbTurn> FilterById(IQueryable<DbTurn> q, TurnId id) {
            var (playerId, turnNumber) = id;
            return q.Where(x => x.PlayerId == playerId && x.Number == turnNumber);
        }

        public int Number { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public bool Ready { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        [GraphQLIgnore]
        public DbPlayer Player { get; set; }

        [GraphQLIgnore]
        public List<DbReport> Reports { get; set; } = new List<DbReport>();

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
    }
}
