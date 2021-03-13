namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("University")]
    public class DbUniversity {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        [Required]
        public string Name { get; set; }

        [GraphQLIgnore]
        public List<DbStudyPlan> Plans { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbUniversityMembership> Members { get; set; } = new ();

        [GraphQLIgnore]
        public DbGame Game { get; set; }
    }
}
