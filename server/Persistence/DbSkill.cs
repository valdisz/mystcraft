namespace advisor.Persistence {
    using advisor.Model;
    using System.ComponentModel.DataAnnotations;

    public class DbSkill : AnSkill {
        [MaxLength(Size.SKILL_CODE)]
        public string Code { get; set; }
        public int? Level { get; set; }
        public int? Days { get; set; }
    }
}
