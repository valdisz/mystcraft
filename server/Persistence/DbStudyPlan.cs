namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("StudyPlan")]
    public class DbStudyPlan : InTurnContext {
        [GraphQLIgnore]
        public int UnitNumber { get; set; }

        [GraphQLIgnore]
        public int TurnNumber { get; set; }

        [GraphQLIgnore]
        public long PlayerId { get; set; }

        public DbSkill Target { get; set; }

        [MaxLength(64)]
        public string Study { get; set; }

        public List<int> Teach { get; set; } = new ();

        [GraphQLIgnore]
        public DbTurn Turn { get; set; }

        public DbUnit Unit { get; set; }
    }
}
