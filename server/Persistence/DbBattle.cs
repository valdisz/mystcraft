namespace advisor.Persistence {
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using advisor.Model;
    using HotChocolate;

    public class DbArmy {
        public int Number { get; set; }

        [Required]
        [MaxLength(Size.LABEL)]
        public string Name { get; set; }
    }

    public class DbBattle : InTurnContext {
        [Key]
        public long Id { get; set; }
        public long PlayerId { get; set; }
        public int TurnNumber { get; set; }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        [Required]
        [MaxLength(Size.LABEL)]
        public string Label { get; set; }

        [Required]
        [MaxLength(Size.PROVINCE)]
        public string Province { get; set; }

        [Required]
        [MaxLength(Size.TERRAIN)]
        public string Terrain { get; set; }

        [Required]
        public DbArmy Attacker { get; set; }

        [Required]
        public DbArmy Defender { get; set; }

        [Required]
        public JBattle Battle { get; set; }

        [GraphQLIgnore]
        public DbPlayerTurn Turn { get; set; }
    }
}
