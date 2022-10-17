namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public class DbAlliance : InGameContext {
        [GraphQLIgnore]
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        [Required]
        [MaxLength(Size.NAME)]
        public string Name { get; set; }

        public List<DbAllianceMember> Members { get; set; } = new ();

        [GraphQLIgnore]
        public DbGame Game { get; set; }
    }
}
