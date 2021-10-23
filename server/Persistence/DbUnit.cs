namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Unit")]
    public class DbUnit : InTurnContext {
        public int Number { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        [GraphQLIgnore]
        public string RegionId { get; set; }

        [GraphQLIgnore]
        public long? StrcutureId { get; set; }

        [GraphQLIgnore]
        public int? FactionNumber { get; set; }

        public int Sequence { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public bool OnGuard { get; set; }

        public List<string> Flags { get; set; } = new List<string>();

        public int? Weight { get; set; }

        [Required]
        public List<DbUnitItem> Items { get; set; } = new ();
        public DbCapacity Capacity { get; set; }
        public List<DbSkill> Skills { get; set; } = new List<DbSkill>();
        public List<DbSkill> CanStudy { get; set; } = new List<DbSkill>();
        public DbItem ReadyItem { get; set; }
        public DbSkill CombatSpell { get; set; }

        public string Orders { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        public DbFaction Faction { get; set; }


        [GraphQLIgnore]
        public DbStructure Structure { get; set; }

        [GraphQLIgnore]
        public DbStudyPlan Plan { get; set; }

        [GraphQLIgnore]
        public List<DbEvent> Events { get; set; } = new ();
    }
}
