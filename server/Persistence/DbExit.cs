namespace atlantis.Persistence
{
    using System.ComponentModel.DataAnnotations;
    using atlantis.Model;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    [GraphQLName("Exit")]
    public class DbExit {
        public DbExit() {

        }

        public DbExit(DbExit other) {
            this.Direction = other.Direction;
            this.Label = other.Label;
            this.Province = other.Province;
            this.Settlement = other.Settlement != null
                ? new DbSettlement {
                    Name = other.Settlement.Name,
                    Size = other.Settlement.Size
                }
                : null;
            this.Terrain = other.Terrain;
            this.X = other.X;
            this.Y = other.Y;
            this.Z = other.Z;
        }

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
