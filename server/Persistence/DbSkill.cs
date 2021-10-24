namespace advisor.Persistence {
    using HotChocolate;
    using System.ComponentModel.DataAnnotations;

    [GraphQLName("Skill")]
    public class DbSkill {
        [MaxLength(8)]
        public string Code { get; set; }
        public int? Level { get; set; }
        public int? Days { get; set; }
    }
}
