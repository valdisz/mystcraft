namespace atlantis.Persistence {
    using Microsoft.EntityFrameworkCore;
    using HotChocolate;

    [Owned]
    [GraphQLName("Skill")]
    public class DbSkill {
        public string Code { get; set; }
        public int? Level { get; set; }
        public int? Days { get; set; }
    }
}
