namespace advisor.Persistence
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;

    public enum DigestAlgorithm {
        SHA256
    }

    [GraphQLName("User")]
    public class DbUser {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Email { get; set; }

        [GraphQLIgnore]
        [Required]
        public string Salt { get; set; }

        [GraphQLIgnore]
        [Required]
        public DigestAlgorithm Algorithm { get; set; }

        [GraphQLIgnore]
        [Required]
        public string Digest { get; set; }

        public List<DbUserRole> Roles { get; set; } = new ();


        [GraphQLIgnore]
        public List<DbUserGame> UserGames { get; set; } = new ();


        [GraphQLIgnore]
        public List<DbUniversity> Universities { get; set; } = new ();

        [GraphQLIgnore]
        public List<DbUniversityUser> UniversityUsers { get; set; } = new ();
    }
}
