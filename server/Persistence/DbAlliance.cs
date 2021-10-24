namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    [GraphQLName("Alliance")]
    public class DbAlliance : InGameContext {
        [Key]
        public long Id { get; set; }

        [GraphQLIgnore]
        public long GameId { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        [GraphQLIgnore]
        public List<DbAllianceMember> Members { get; set; } = new ();

        [GraphQLIgnore]
        public DbGame Game { get; set; }
    }
}
