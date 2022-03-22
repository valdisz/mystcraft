namespace advisor.Persistence {
    using advisor.Model;
    using System.ComponentModel.DataAnnotations;

    public class DbSkill : AnSkill {
        [MaxLength(8)]
        public string Code { get; set; }
        public int? Level { get; set; }
        public int? Days { get; set; }
    }
}
