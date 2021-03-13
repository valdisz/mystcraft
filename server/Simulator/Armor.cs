using System.Collections.Generic;
using System.Linq;

namespace advisor.Simulator
{
    public class Armor {
        public Armor(params (AttackType, int)[] protection) {
            Protection = protection.ToDictionary(k => k.Item1, v => v.Item2);
        }

        public Dictionary<AttackType, int> Protection { get; }

        public static readonly Armor PLAA = new Armor(
            (AttackType.Slashing, 66),
            (AttackType.Piercing, 66),
            (AttackType.Crushing, 50),
            (AttackType.Cleaaving, 66),
            (AttackType.ArmorPiercing, 25),
            (AttackType.Energy, 0),
            (AttackType.Spirit, 0),
            (AttackType.Weather, 0)
        );

        public static readonly Armor PADA = new Armor(
            (AttackType.Slashing,       15),
            (AttackType.Piercing,       10),
            (AttackType.Crushing,       15),
            (AttackType.Cleaaving,      15),
            (AttackType.ArmorPiercing,  0),
            (AttackType.Energy,         0),
            (AttackType.Spirit,         0),
            (AttackType.Weather,        0)
        );

        public static readonly Armor LEAA = new Armor(
            (AttackType.Slashing,       33),
            (AttackType.Piercing,       15),
            (AttackType.Crushing,       25),
            (AttackType.Cleaaving,      25),
            (AttackType.ArmorPiercing,  0),
            (AttackType.Energy,         0),
            (AttackType.Spirit,         0),
            (AttackType.Weather,        0)
        );

        public static readonly Armor SCAA = new Armor(
            (AttackType.Slashing,       33),
            (AttackType.Piercing,       33),
            (AttackType.Crushing,       33),
            (AttackType.Cleaaving,      25),
            (AttackType.ArmorPiercing,  0),
            (AttackType.Energy,         0),
            (AttackType.Spirit,         0),
            (AttackType.Weather,        0)
        );

        public static readonly Armor MAIA = new Armor(
            (AttackType.Slashing,       33),
            (AttackType.Piercing,       50),
            (AttackType.Crushing,       25),
            (AttackType.Cleaaving,      33),
            (AttackType.ArmorPiercing,  0),
            (AttackType.Energy,         0),
            (AttackType.Spirit,         0),
            (AttackType.Weather,        0)
        );

        public static readonly Armor MSCA = new Armor(
            (AttackType.Slashing,       50),
            (AttackType.Piercing,       50),
            (AttackType.Crushing,       50),
            (AttackType.Cleaaving,      33),
            (AttackType.ArmorPiercing,  15),
            (AttackType.Energy,         15),
            (AttackType.Spirit,         15),
            (AttackType.Weather,        15)
        );

        public static readonly Armor MMAA = new Armor(
            (AttackType.Slashing,       50),
            (AttackType.Piercing,       66),
            (AttackType.Crushing,       50),
            (AttackType.Cleaaving,      50),
            (AttackType.ArmorPiercing,  25),
            (AttackType.Energy,         15),
            (AttackType.Spirit,         15),
            (AttackType.Weather,        15)
        );

        public static readonly Armor MPMA = new Armor(
            (AttackType.Slashing,       66),
            (AttackType.Piercing,       66),
            (AttackType.Crushing,       50),
            (AttackType.Cleaaving,      66),
            (AttackType.ArmorPiercing,  33),
            (AttackType.Energy,         25),
            (AttackType.Spirit,         25),
            (AttackType.Weather,        25)
        );

        public static readonly Armor MPLA = new Armor(
            (AttackType.Slashing,       75),
            (AttackType.Piercing,       75),
            (AttackType.Crushing,       66),
            (AttackType.Cleaaving,      75),
            (AttackType.ArmorPiercing,  50),
            (AttackType.Energy,         33),
            (AttackType.Spirit,         33),
            (AttackType.Weather,        33)
        );

        public static readonly Armor ESCA = new Armor(
            (AttackType.Slashing,       50),
            (AttackType.Piercing,       50),
            (AttackType.Crushing,       50),
            (AttackType.Cleaaving,      33),
            (AttackType.ArmorPiercing,  15),
            (AttackType.Energy,         15),
            (AttackType.Spirit,         15),
            (AttackType.Weather,        15)
        );
    }
}
