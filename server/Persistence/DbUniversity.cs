namespace atlantis.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("University")]
    public class DbUniversity {
        [Key]
        public long Id { get; set; }

        public long GameId { get; set; }

        [GraphQLIgnore]
        public List<DbStudyPlan> Plans { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbUser> Users { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbUniversityUser> UniversityUsers { get; set; } = new ();

        [GraphQLIgnore]
        public DbGame Game { get; set; }
    }
}
