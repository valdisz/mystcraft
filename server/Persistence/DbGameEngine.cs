namespace advisor.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public class DbGameEngine {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(Size.LABEL)]
        public string Name { get; set; }

        [Required]
        public DateTimeOffset CreatedAt { get; set; }

        [Required]
        [GraphQLIgnore]
        public byte[] Contents { get; set; }

        [GraphQLIgnore]
        public List<DbGame> Games { get; set; } = new ();
    }
}
