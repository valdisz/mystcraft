using System;
using System.Collections.Generic;
using System.Linq;

namespace atlantis.Simulator {
    public enum AttackType {
        Piercing,
        Slashing,
        Crushing,
        ArmorPiercing,
        Energy,
        Spirit,
        Weather,
        Cleaaving
    }

    public enum WeaponLength {
        None = 0,
        Short = 1,
        Normal = 2,
        Long = 3
    }

    public class Soldier {
        public Soldier(int hp, int combat, int riding, Weapon weapon, Armor armor) {
            Hp = hp;
            Combat = combat;
            Riding = riding;
            Weapon = weapon;
            Armor = armor;
        }

        public int Hp { get; private set; }
        public int Combat { get; }
        public int Riding { get; }
        public Weapon Weapon { get; }
        public Armor Armor { get; }
        public int Cooldown { get; private set; }

        public bool CanAttack => Cooldown == 0;

        public bool HasWeaponAdvantage(Soldier target) {
            int aLen = (int) (Weapon?.Length ?? WeaponLength.None);
            int dLen = (int) (target?.Weapon?.Length ?? WeaponLength.None);

            return aLen > dLen;
        }

        public int GetAttackLevel(Soldier target) {
            int level = Combat;
            if (Weapon != null) {
                level += Weapon.Attack;

                if (Weapon.Riding) level += Riding;
            }

            if (HasWeaponAdvantage(target)) level += 1;

            return level;
        }

        public int GetDefenseLevel(Soldier attacker) {
            int level = Combat;
            if (Weapon != null) {
                level += Weapon.Defense;

                if (Weapon.Riding) level += Riding;
            }

            if (HasWeaponAdvantage(attacker)) level += 1;

            return level;
        }

        public int GetProtectionLevel(AttackType attack) {
            return Armor?.Protection[attack] ?? 0;
        }

        public bool Hit() {
            Hp--;
            return Hp > 0;
        }

        public void BeforeRound() {
            if (Cooldown > 0) Cooldown--;
        }

        public void UseWeapon() {
            Cooldown = Weapon?.Cooldown ?? 1;
        }
    }

    public class Unit {
        public Unit(int count, int hp, int combat, int riding, (Weapon, int)[] weapons, (Armor, int)[] armor) {
            Count = count;
            Hp = hp;
            Combat = combat;
            Riding = riding;
            Weapons = weapons ?? new (Weapon, int)[0];
            Armor = armor ?? new (Armor, int)[0];
        }

        public int Count { get; }
        public int Hp { get; }
        public int Combat { get; }
        public int Riding { get; }
        public (Weapon, int)[] Weapons { get; }
        public (Armor, int)[] Armor { get; }

        public IEnumerable<Weapon> GetWeapons() {
            foreach (var (weapon, count) in Weapons) {
                for (var i = 0; i < count; i++) yield return weapon;
            }
        }

        public IEnumerable<Armor> GetArmor() {
            foreach (var (armor, count) in Armor) {
                for (var i = 0; i < count; i++) yield return armor;
            }
        }

        public IEnumerable<Soldier> GetSoldiers() {
            throw new NotImplementedException();

            // using var weapon = GetWeapons().GetEnumerator();
            // using var armor = GetArmor().GetEnumerator();

            // for (var i = 0; i < Count; i++) {
            //     var w = weapon.MoveNext() ? weapon.Current : null;
            //     var a = armor.MoveNext() ? weapon.Current : null;

            //     yield return new Soldier(Hp, Combat, Riding, w, a);
            // }
        }
    }

    public class Army {
        public Army(string name, int tactics, params Unit[] units) {
            Name = name;
            Tactics = tactics;
            Units = units ?? new Unit[0];
        }

        public string Name { get; }
        public int Tactics { get; }
        public Unit[] Units { get; }

        public IEnumerable<Soldier> GetSoldiers() {
            foreach (var unit in Units) {
                foreach (var s in unit.GetSoldiers()) yield return s;
            }
        }
    }

    public interface ICombat {
        bool Hit(int attackLevel, int defenseLevel);
    }

    public class ExponentialCombat : ICombat {
        private readonly Random rng = new Random();

        public bool Hit(int attackLevel, int defenseLevel) {
            int toHit = 1;
            int toMiss = 1;

            if (attackLevel > defenseLevel) {
                toHit = (int) Math.Pow(2, attackLevel - defenseLevel);
            }
            else if (defenseLevel > attackLevel) {
                toMiss = (int) Math.Pow(2, defenseLevel - attackLevel);
            }

            return rng.Next(toHit + toMiss) < toHit;
        }
    }

    public class LinearCombat : ICombat {
        private readonly Random rng = new Random();

        public bool Hit(int attackLevel, int defenseLevel) {
            int toHit = attackLevel;
            int toMiss = defenseLevel;

            if (toHit < 1) {
                toMiss += Math.Abs(toHit) + 1;
                toHit = 1;
            }

            if (toMiss < 1) {
                toHit += Math.Abs(toMiss) + 1;
                toMiss = 1;
            }

            return rng.Next(toHit + toMiss) < toHit;
        }
    }

    public class BattleStats {
        public int MenLost { get; private set; }
        public int HpLost { get; private set; }

        public void RecordHit(Soldier s) {
            HpLost++;
            if (s.Hp == 0) MenLost++;
        }

        public void UnionWith(BattleStats other) {
            HpLost += other.HpLost;
            MenLost += other.MenLost;
        }
    }

    public class BattleRound {
        public BattleRound(Random rng, Soldier[] sideA, Soldier[] sideB, bool sideBCanAttack = true) {
            this.sideA = new List<Soldier>(sideA);
            this.sideB = new List<Soldier>(sideB);
            this.rng = rng;
            this.sideBCanAttack = sideBCanAttack;
        }

        private readonly List<Soldier> sideA;
        private readonly List<Soldier> sideB;
        private readonly Random rng;
        private readonly bool sideBCanAttack;

        public BattleStats SideA { get; } = new BattleStats();
        public BattleStats SideB { get; } = new BattleStats();

        public Soldier PickAttacker() {
            int possibleAttackers = sideA.Count + (sideBCanAttack ? sideB.Count : 0);
            if (possibleAttackers <= 0) return null;

            int i = rng.Next(possibleAttackers);

            throw new NotImplementedException();
        }

        public void Run(Random rng) {

        }
    }

    public class Battle {
        public Battle(Army a, Army b, ICombat combat) {
            SideA = a;
            SideB = b;
            this.combat = combat;
        }

        private readonly ICombat combat;

        public Army SideA { get; }
        public Army SideB { get; }

        public void RunBattle() {
            List<Soldier> aSoldiers = SideA.GetSoldiers().ToList();
            List<Soldier> bSoldiers = SideB.GetSoldiers().ToList();

            BattleStats aStats = new BattleStats();
            BattleStats bStats = new BattleStats();

            int aTotalHp = aSoldiers.Sum(x => x.Hp);
            int bTotalHp = bSoldiers.Sum(x => x.Hp);

            if (SideA.Tactics > SideB.Tactics) {
                var stats = RunFreeRound(aSoldiers, bSoldiers);
                bStats.UnionWith(stats);
            }
            else if (SideB.Tactics > SideA.Tactics) {
                var stats = RunFreeRound(bSoldiers, aSoldiers);
                aStats.UnionWith(stats);
            }
        }

        private BattleStats RunFreeRound(List<Soldier> aSoldiers, List<Soldier> bSoldiers)
        {
            throw new NotImplementedException();
        }

        private (BattleStats, BattleStats) RunRound(List<Soldier> aSoldiers, List<Soldier> bSoldiers) {
            throw new NotImplementedException();
        }
    }
}
