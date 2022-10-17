namespace advisor.Persistence
{
    using System;
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
        [MaxLength(Size.EMAIL)]
        public string Email { get; set; }

        [GraphQLIgnore]
        [MaxLength(Size.SALT)]
        public string Salt { get; set; }

        [GraphQLIgnore]
        public DigestAlgorithm Algorithm { get; set; }

        [GraphQLIgnore]
        [MaxLength(Size.DIGEST)]
        public string Digest { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LastLoginAt { get; set; }

        public List<string> Roles { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbRegistration> Registrations { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbPlayer> Players { get; set; } = new ();
    }
}
