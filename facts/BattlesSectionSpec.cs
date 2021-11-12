namespace advisor.facts {
    using System.IO;
    using System.Threading.Tasks;
    using Xunit;

    public class BattlesSectionSpec {
        const string BATTLES = @"Battles during turn:
Sandlings (384) attacks Mystic Masons (15876) in desert (50,28) in
  Feapi'iss!

Attackers:
Sandlings (384), 7 sandlings [SAND] (Combat 3/3, Attacks 2, Hits 2,
  Tactics 1).

Defenders:
surveyor (4637), Nomads (82), behind, gnoll [GNOL].
Nookers (12462), Nomads (82), behind, 14 gnolls [GNOL], 13 humans
  [HUMN], combat 1.
South-West Feapi`iss Farmer Guild  (12918), Nomads (82), behind, 6
  humans [HUMN].
Cameleers (10938), Nomads (82), behind, 4 gnolls [GNOL], 32 camels
  [CAME].
Mystic Masons (15876), Nomads (82), behind, 2 hill dwarves [HDWA], 2
  picks [PICK].
Miners (16212), Nomads (82), behind, 6 humans [HUMN], 6 picks [PICK].
masons3 (17190), Nomads (82), behind, 3 goblins [GBLN], 4 picks
  [PICK].
for nookers (8466), Nomads (82), behind, 5 humans [HUMN], combat 1.
Yamshy (16665), Nomads (82), behind, human [HUMN], 405 camels [CAME].
Cameleers (10940), Nomads (82), behind, 2 gnolls [GNOL].
Camel/Carp (11566), Nomads (82), behind, 2 gnolls [GNOL].
quarrier 2 (11663), Nomads (82), behind, hill dwarf [HDWA], pick
  [PICK].
quarrier (11575), Nomads (82), behind, goblin [GBLN].
Vainamoinen (17514), Nomads (82), behind, leader [LEAD].
Gilgamesh (8455), Nomads (82), behind, leader [LEAD], tactics 5.
Archers (9315), Nomads (82), behind, 13 wood elves [WELF], 13 longbows
  [LBOW], longbow 5.
Arbalesters (9320), Nomads (82), behind, 13 goblins [GBLN], 13
  crossbows [XBOW], crossbow 5.
Tumen (9044), Nomads (82), 10 iron shields [ISHD], 10 plate armor
  [PARM], 10 gnolls [GNOL], 10 admantium swords [ASWR], combat 4.
Nookers (10082), Nomads (82), 111 gnolls [GNOL], hill dwarf [HDWA],
  126 swords [SWOR], 125 plate armor [PARM], 85 iron shields [ISHD],
  300 camels [CAME], combat 1.
for nookers (5450), Nomads (82), 19 gnolls [GNOL], combat 3.
Tumen (5459), Nomads (82), 50 gnolls [GNOL], 50 swords [SWOR], 50
  plate armor [PARM], combat 3.
Tumen (5462), Nomads (82), 50 gnolls [GNOL], 50 plate armor [PARM], 50
  swords [SWOR], combat 3.
Tumen (5463), Nomads (82), 50 gnolls [GNOL], 50 plate armor [PARM], 50
  swords [SWOR], 49 iron shields [ISHD], combat 3.

Round 1:
Mystic Masons (15876) tactics bonus 3.

Sandlings (384) loses 7.
Mystic Masons (15876) loses 0.

Round 1 statistics:

Sandlings (384) army:
- Sandlings (384):
  - without weapon (slashing melee attack), attacked 2 of 2 times, 0
  successful attack, 0 blocked by armor, 0 hit, 0 total damage, and
  killed 0 enemy.

Mystic Masons (15876) army:
- Tumen (5459):
  - sword [SWOR] (slashing melee attack), attacked 4 of 8 times, 4
  successful attacks, 0 blocked by armor, 4 hit, 4 total damage, and
  killed 1 enemy.
- Tumen (5462):
  - sword [SWOR] (slashing melee attack), attacked 4 of 10 times, 4
  successful attacks, 0 blocked by armor, 4 hits, 4 total damage, and
  killed 2 enemies.
- Tumen (5463):
  - sword [SWOR] (slashing melee attack), attacked 3 of 8 times, 2
  successful attacks, 0 blocked by armor, 2 hits, 2 total damage, and
  killed 2 enemies.
- Nookers (10082):
  - sword [SWOR] (slashing melee attack), attacked 5 of 9 times, 4
  successful attacks, 0 blocked by armor, 4 hits, 4 total damage, and
  killed 2 enemies.

Sandlings (384) is destroyed!

Battle statistics:

Sandlings (384) army:
- Sandlings (384):
  - without weapon (slashing melee attack), attacked 2 of 2 times, 0
  successful attack, 0 blocked by armor, 0 hit, 0 total damage, and
  killed 0 enemy.

Mystic Masons (15876) army:
- Tumen (5459):
  - sword [SWOR] (slashing melee attack), attacked 4 of 8 times, 4
  successful attacks, 0 blocked by armor, 4 hit, 4 total damage, and
  killed 1 enemy.
- Tumen (5462):
  - sword [SWOR] (slashing melee attack), attacked 4 of 10 times, 4
  successful attacks, 0 blocked by armor, 4 hits, 4 total damage, and
  killed 2 enemies.
- Tumen (5463):
  - sword [SWOR] (slashing melee attack), attacked 3 of 8 times, 2
  successful attacks, 0 blocked by armor, 2 hits, 2 total damage, and
  killed 2 enemies.
- Nookers (10082):
  - sword [SWOR] (slashing melee attack), attacked 5 of 9 times, 4
  successful attacks, 0 blocked by armor, 4 hits, 4 total damage, and
  killed 2 enemies.

Total Casualties:
Sandlings (384) loses 7.
Damaged units: 384.
Mystic Masons (15876) loses 0.

Spoils: crossbow [XBOW], 5 camels [CAME], 3 furs [FUR], 205 silver
  [SILV].

";

        [Fact]
        public async Task CanParseBattles() {
            using var source = new StringReader(File.ReadAllText(@"/home/valdis/projects/advisor/facts/battle.txt"));
            using var converter = new AtlantisReportJsonConverter(
                source,
                new BattlesSection()
            );

            var json = await converter.ReadAsJsonAsync();


            File.WriteAllText("battle.json", json.ToString());
        }
    }
}
