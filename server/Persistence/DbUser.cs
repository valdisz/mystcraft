namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public enum DigestAlgorithm {
        SHA256
    }

    public class DbUser {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(128)]
        public string Email { get; set; }

        [GraphQLIgnore]
        [MaxLength(32)]
        public string Salt { get; set; }

        [GraphQLIgnore]
        public DigestAlgorithm Algorithm { get; set; }

        [GraphQLIgnore]
        [MaxLength(128)]
        public string Digest { get; set; }

        public List<string> Roles { get; set; } = new ();


        [GraphQLIgnore]
        public List<DbPlayer> Players { get; set; } = new ();
    }
}
