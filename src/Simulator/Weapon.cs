namespace atlantis.Simulator
{
    public class Weapon {
        public Weapon(AttackType type, WeaponLength length, int attacks, int cooldown, bool riding, int attack, int defense) {
            Type = type;
            Length = length;
            Attacks = attacks;
            Cooldown = cooldown;
            Attack = attack;
            Defense = defense;
            Riding = riding;
        }

        public AttackType Type { get; }
        public WeaponLength Length { get; }
        public int Attacks { get; }
        public int Cooldown { get; }
        public bool Riding { get; }
        public int Attack { get; }
        public int Defense { get; }

        public static readonly Weapon MSWO = new Weapon(
            type:       AttackType.Slashing,
            length:     WeaponLength.Normal,
            attacks:    1,
            cooldown:   1,
            riding:     true,
            attack:     4,
            defense:    4
        );

        public static readonly Weapon SWOR = new Weapon(
            type:       AttackType.Slashing,
            length:     WeaponLength.Normal,
            attacks:    1,
            cooldown:   1,
            riding:     true,
            attack:     2,
            defense:    2
        );

        public static readonly Weapon DAGG = new Weapon(
            type:       AttackType.Piercing,
            length:     WeaponLength.Short,
            attacks:    2,
            cooldown:   1,
            riding:     false,
            attack:     1,
            defense:    1
        );

        public static readonly Weapon PICK = new Weapon(
            type:       AttackType.Piercing,
            length:     WeaponLength.Normal,
            attacks:    1,
            cooldown:   1,
            riding:     true,
            attack:     1,
            defense:    1
        );

        public static readonly Weapon SPEA = new Weapon(
            type:       AttackType.Piercing,
            length:     WeaponLength.Long,
            attacks:    1,
            cooldown:   1,
            riding:     true,
            attack:     2,
            defense:    2
        );

        public static readonly Weapon AXE = new Weapon(
            type:       AttackType.Cleaaving,
            length:     WeaponLength.Normal,
            attacks:    1,
            cooldown:   1,
            riding:     true,
            attack:     2,
            defense:    2
        );

        public static readonly Weapon HAMM = new Weapon(
            type:       AttackType.Crushing,
            length:     WeaponLength.Normal,
            attacks:    1,
            cooldown:   1,
            riding:     true,
            attack:     2,
            defense:    2
        );

        public static readonly Weapon MACE = new Weapon(
            type:       AttackType.Crushing,
            length:     WeaponLength.Normal,
            attacks:    1,
            cooldown:   1,
            riding:     true,
            attack:     2,
            defense:    2
        );

        public static readonly Weapon _2HSWO = new Weapon(
            type:       AttackType.Slashing,
            length:     WeaponLength.Long,
            attacks:    1,
            cooldown:   2,
            riding:     false,
            attack:     4,
            defense:    1
        );

        public static readonly Weapon MAUL = new Weapon(
            type:       AttackType.Crushing,
            length:     WeaponLength.Long,
            attacks:    1,
            cooldown:   2,
            riding:     true,
            attack:     4,
            defense:    1
        );
    }
}
