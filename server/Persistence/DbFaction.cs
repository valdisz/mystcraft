namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using advisor.Model;
    using HotChocolate;

    using FactionId = System.ValueTuple<long, int, int>;

    public class DbFaction : InTurnContext {
        [GraphQLIgnore]
        [NotMapped]
        public FactionId CompsiteId => MakeId(this);

        public static FactionId MakeId(long playerId, int turnNumber, int factionNumber) => (playerId, turnNumber, factionNumber);
        public static FactionId MakeId(DbFaction faction) => (faction.PlayerId, faction.TurnNumber, faction.Number);

        public static IQueryable<DbFaction> FilterById(IQueryable<DbFaction> q, FactionId id) {
            var (playerId, turnNumber, factionNumber) = id;
            return q.Where(x =>
                    x.PlayerId == playerId
                && x.TurnNumber == turnNumber
                && x.Number == factionNumber
            );
        }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [Required]
        public int Number { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public Stance? DefaultAttitude { get; set; }

        public List<DbAttitude> Attitudes { get; set; } = new List<DbAttitude>();

        [GraphQLIgnore]
        public DbPlayerTurn Turn { get; set; }

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new List<DbEvent>();

        [GraphQLIgnore]
        public List<DbUnit> Units { get; set; } = new List<DbUnit>();
    }
}
