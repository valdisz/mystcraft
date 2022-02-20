namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using HotChocolate;

    public record UnitId(long PlayerId, int TurnNumber, int UnitNumber);

    [GraphQLName("Unit")]
    public class DbUnit : InTurnContext {
        [GraphQLIgnore]
        [NotMapped]
        public string CompositeId => MakeId(this);

        public static string MakeId(DbUnit unit) => MakeId(unit.PlayerId, unit.TurnNumber, unit.Number);
        public static string MakeId(long playerId, int turnNumber, int unitNumber) => $"{playerId}/{turnNumber}/{unitNumber}";
        public static UnitId ParseId(string id) {
            var segments = id.Split("/");
            return new (
                long.Parse(segments[0]),
                int.Parse(segments[1]),
                int.Parse(segments[2])
            );
        }

        public static IQueryable<DbUnit> FilterById(IQueryable<DbUnit> q, UnitId id) {
            return q.Where(x =>
                    x.PlayerId == id.PlayerId
                && x.TurnNumber == id.TurnNumber
                && x.Number == id.UnitNumber
            );
        }

        public int Number { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        [MaxLength(14)]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        [MaxLength(24)]
        public string StrcutureId { get; set; }

        public int? StructureNumber { get; set; }

        public int? FactionNumber { get; set; }

        public int Sequence { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [MaxLength(256)]
        public string Description { get; set; }

        [Required]
        public bool OnGuard { get; set; }

        public List<string> Flags { get; set; } = new List<string>();

        public int? Weight { get; set; }

        [Required]
        public List<DbUnitItem> Items { get; set; } = new ();

        public DbCapacity Capacity { get; set; }

        public List<DbSkill> Skills { get; set; } = new List<DbSkill>();

        public List<string> CanStudy { get; set; } = new List<string>();

        [MaxLength(8)]
        public string ReadyItem { get; set; }

        [MaxLength(8)]
        public string CombatSpell { get; set; }

        public string Orders { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        [GraphQLIgnore]
        public DbFaction Faction { get; set; }

        [GraphQLIgnore]
        public DbStructure Structure { get; set; }

        public DbStudyPlan StudyPlan { get; set; }

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new ();
    }
}
