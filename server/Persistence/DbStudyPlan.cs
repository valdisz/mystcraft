namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("StudyPlan")]
    public class DbStudyPlan {
        [Key]
        public long Id { get; set; }

        public long UniversityId { get; set; }
        public long TurnId { get; set; }
        public long UnitId { get; set; }

        public DbSkill Target { get; set; }

        public string Learn { get; set; }

        public List<long> Teach { get; set; } = new ();

        public DbUniversity University { get; set; }
        public DbTurn Turn { get; set; }
        public DbUnit Unit { get; set; }
    }
}
