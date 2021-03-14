namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Unit")]
    public class DbUnit {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long TurnId { get; set; }

        [GraphQLIgnore]
        public long RegionId { get; set; }

        [GraphQLIgnore]
        public long? StrcutureId { get; set; }

        [GraphQLIgnore]
        public long? FactionId { get; set; }

        [GraphQLIgnore]
        public int Sequence { get; set; }

        public int Number { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public bool OnGuard { get; set; }

        public List<string> Flags { get; set; } = new List<string>();

        public int? Weight { get; set; }

        [Required]
        public List<DbItem> Items { get; set; } = new List<DbItem>();
        public DbCapacity Capacity { get; set; }
        public List<DbSkill> Skills { get; set; } = new List<DbSkill>();
        public List<DbSkill> CanStudy { get; set; } = new List<DbSkill>();
        public DbItem ReadyItem { get; set; }
        public DbSkill CombatSpell { get; set; }

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        [GraphQLIgnore]
        public DbRegion Region { get; set; }

        public DbFaction Faction { get; set; }


        [GraphQLIgnore]
        public DbStructure Structure { get; set; }

        [GraphQLIgnore]
        public DbStudyPlan Plan { get; set; }
    }
}
