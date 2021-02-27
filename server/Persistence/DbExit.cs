namespace atlantis.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using atlantis.Model;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    [GraphQLName("Exit")]
    public class DbExit {
        [GraphQLIgnore]
        public string RegionUID => DbRegion.GetUID(X, Y, Z);

        [Required]
        public Direction Direction { get; set; }

        [Required]
        public int X { get; set; }

        [Required]
        public int Y { get; set; }

        [Required]
        public int Z { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public string Province { get; set; }

        [Required]
        public string Terrain { get; set; }

        public DbSettlement Settlement { get; set; }
    }
}
