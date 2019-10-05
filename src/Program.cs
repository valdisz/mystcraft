namespace atlantis
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Pidgin;
    using static Tokens;

    class Program
    {
        public static void ParaseStructure() {
string input = @"- Herdsmen of the Ungre Guild (1767), 3 humans [MAN], 3 spices [SPIC],
  2 gems [GEM], net [NET], 2 mink [MINK], 3 livestock [LIVE]; Content
  looking shepherds and herdsmen.
";

            var result = AtlantisParser.Unit<Pidgin.Unit>().Parse(input);
            AssertParsed(result);

            if (result.Success) {
                Console.WriteLine(result.Value.ToString());
                Console.WriteLine();
                Console.WriteLine("OK");
            }
        }

        public static void ParseRegion() {
            string input = @"mountain (0,4) in Davale, contains Sachead [city], 19453 peasants
  (hill dwarves), $31513.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $18.1 (Max: $6302).
  Wanted: 120 grain [GRAI] at $18, 134 livestock [LIVE] at $23, 188
    fish [FISH] at $26, 24 cloth armor [CLAR] at $80, 28 leather armor
    [LARM] at $83, 66 silk [SILK] at $288, 35 dye [DYE] at $240.
  For Sale: 61 ivory [IVOR] at $88, 49 wine [WINE] at $94, 778 hill
    dwarves [HDWA] at $57, 155 leaders [LEAD] at $1013.
  Entertainment available: $1770.
  Products: 34 grain [GRAI], 21 iron [IRON], 10 stone [STON].

Exits:
  North : mountain (0,2) in Davale.
  Northeast : mountain (1,3) in Davale.
  Southeast : mountain (1,5) in Davale.
  South : mountain (0,6) in Davale.
  Southwest : mountain (55,5) in Davale.
  Northwest : mountain (55,3) in Davale.

- production miners (736), Demon's Duchy (28), avoiding, behind, 7
  hill dwarves [HDWA], 5 picks [PICK], 128 iron [IRON], 3 mithril
  [MITH].
- production weapons (993), Demon's Duchy (28), avoiding, behind, 5
  hill dwarves [HDWA], 20 hammers [HAMM], 15 picks [PICK], 268 swords
  [SWOR].
- Ambassador Sachead (1104), Disasters Inc (43), avoiding, behind,
  revealing faction, orc [ORC], 2 hill dwarves [HDWA], sword [SWOR].
  Weight: 31. Capacity: 0/0/45/0. Skills: entertainment [ENTE] 1 (30).
- production stone (1265), Demon's Duchy (28), avoiding, behind, 5
  hill dwarves [HDWA], 5 picks [PICK], 70 stone [STON].
- Unit (1699), Semigallians (18), avoiding, behind, human [MAN].
- production farmers (1760), Demon's Duchy (28), avoiding, behind, 10
  humans [MAN], 3 swords [SWOR], 5 bags [BAG], 34 grain [GRAI].
- Erzherzog (60), Erzherzogtum Motsognir (74), avoiding, leader
  [LEAD].
- Meister (4982), Erzherzogtum Motsognir (74), avoiding, hill dwarf
  [HDWA].
- Hauptmann (4983), Erzherzogtum Motsognir (74), leader [LEAD].
- Spielleute (5345), Erzherzogtum Motsognir (74), avoiding, 10 hill
  dwarves [HDWA].
- general (3835), Demon's Duchy (28), behind, hill dwarf [HDWA].
- Ambassador (1410), Noizy Tribe (49), avoiding, behind, lizardman
  [LIZA]; Heading to Sachead to become an Ambassador.
- administration sachead (7128), on guard, Disasters Inc (43),
  revealing faction, sharing, 277 hill dwarves [HDWA], 265 orcs [ORC],
  12 horses [HORS], 55678 silver [SILV]. Weight: 6020. Capacity:
  0/840/8970/0. Skills: combat [COMB] 1 (31).
- administration (7464), Demon's Duchy (28), avoiding, behind, 15 hill
  dwarves [HDWA], 61 horses [HORS], 50 livestock [LIVE].
- production rootstone (7465), Demon's Duchy (28), avoiding, behind,
  20 hill dwarves [HDWA].
- combat (4761), Demon's Duchy (28), 50 centaurs [CTAU], 50 swords
  [SWOR], 70 chain armor [CARM].
- Leader Weaponsmith (738), Demon's Duchy (28), avoiding, behind,
  leader [LEAD].
- Leader Armorsmith (3283), Demon's Duchy (28), avoiding, behind,
  leader [LEAD].
- mountain men (5534), Disasters Inc (43), revealing faction, sharing,
  50 orcs [ORC]. Weight: 500. Capacity: 0/0/750/0. Skills: combat
  [COMB] 2 (151).
- Leader Combat (4205), Demon's Duchy (28), avoiding, behind, 4
  leaders [LEAD], 2 horses [HORS].
- xbowers (5755), Demon's Duchy (28), behind, 20 gnomes [GNOM], 3
  horses [HORS].
- gajejs (7105), Skalperians (22), avoiding, behind, hill dwarf
  [HDWA].
- training miners (7893), Demon's Duchy (28), avoiding, behind, 30
  hill dwarves [HDWA].
- Unit (7894), Demon's Duchy (28), avoiding, behind, leader [LEAD].
- Unit (7895), Demon's Duchy (28), behind, leader [LEAD].
- training armorers (7896), Demon's Duchy (28), avoiding, behind, 30
  hill dwarves [HDWA].
- Unit (7892), Demon's Duchy (28), behind, leader [LEAD].
- Leader Horsetraining (3838), Demon's Duchy (28), avoiding, behind,
  leader [LEAD], 11 horses [HORS].
- Leader Builder (739), Demon's Duchy (28), avoiding, behind, leader
  [LEAD].
- Leader Armorsmith (4483), Demon's Duchy (28), avoiding, behind,
  leader [LEAD].
- Leader Combat (4486), Demon's Duchy (28), avoiding, behind, leader
  [LEAD].
- Leader Carpenter (4757), Demon's Duchy (28), avoiding, behind,
  leader [LEAD].
- Leader Lumberjack (4758), Demon's Duchy (28), avoiding, behind,
  leader [LEAD].
- combat (4484), Demon's Duchy (28), avoiding, behind, 40 gnomes
  [GNOM].
- Schauspieler (8543), Erzherzogtum Motsognir (74), avoiding, 10 hill
  dwarves [HDWA].
- mountain smart orc (5535), Disasters Inc (43), behind, revealing
  faction, weightless battle spoils, orc [ORC], horse [HORS]. Weight:
  60. Capacity: 0/70/85/0. Skills: tactics [TACT] 2 (90), riding
  [RIDI] 2 (90), observation [OBSE] 2 (90).
- Holzwurm (6927), Erzherzogtum Motsognir (74), avoiding, 5 centaurs
  [CTAU].
* Scout (2512), Avalon Empire (15), avoiding, behind, revealing
  faction, holding, receiving no aid, won't cross water, centaur
  [CTAU], 7 silver [SILV]. Weight: 50. Capacity: 0/70/70/0. Skills:
  stealth [STEA] 1 (30), riding [RIDI] 1 (40).
- Unit (9257), Disasters Inc (43), avoiding, behind, revealing
  faction, high elf [HELF], 4 winged horses [WING], 240 silver [SILV].
  Weight: 210. Capacity: 280/280/295/0. Skills: none.
- courier Sachead-N (2794), Demon's Duchy (28), avoiding, behind,
  human [MAN], horse [HORS].
- courier Sachead-SE (3568), Demon's Duchy (28), avoiding, behind, orc
  [ORC], horse [HORS], iron [IRON].
- wagoneer Sachead-NE (3285), Demon's Duchy (28), avoiding, behind,
  orc [ORC], horse [HORS], wagon [WAGO], spinning wheel [SPIN], 10
  iron [IRON].
- wagoneer Sachead-NW (3569), Demon's Duchy (28), avoiding, behind,
  orc [ORC], 50 swords [SWOR], horse [HORS], wagon [WAGO], 4 bags
  [BAG], 10 iron [IRON].
- courier Sachead-SW (1267), Demon's Duchy (28), avoiding, behind,
  hill dwarf [HDWA], 3 horses [HORS], 50 livestock [LIVE], mithril
  [MITH], iron [IRON].
- p mithril (9252), Disasters Inc (43), avoiding, behind, revealing
  faction, sharing, 4 hill dwarves [HDWA], 2 horses [HORS], 160 silver
  [SILV]. Weight: 140. Capacity: 0/140/200/0. Skills: mining [MINI] 4
  (330).
- courier Sachead-2W (3294), Demon's Duchy (28), avoiding, behind,
  high elf [HELF], horse [HORS], iron [IRON].

+ Building [1] : Fort.
  - Unit (469), Demon's Duchy (28), behind, leader [LEAD].

+ Building [2] : Fort.
  - Unit (3831), Demon's Duchy (28), behind, leader [LEAD].

+ Building [4] : Road S.
  - mountain (8323), Disasters Inc (43), revealing faction, sharing,
    100 hill dwarves [HDWA]. Weight: 1000. Capacity: 0/0/1500/0.
    Skills: combat [COMB] 1 (30).

+ Building [5] : Fort.
  - production builders (3284), Demon's Duchy (28), avoiding, behind,
    10 gnomes [GNOM].
  - Unit (3832), Demon's Duchy (28), behind, leader [LEAD].


";

            var result = AtlantisParser.Region.Parse(input);
            AssertParsed(result);

            if (result.Success) {
                Console.WriteLine(result.Value.ToString());
                Console.WriteLine();
                Console.WriteLine("OK");
            }
        }

        static void Main(string[] args)
        {
            // ParaseStructure(); return;
            // ParseRegion(); return;

            var input = File.ReadAllText(@"C:\local\var\git-private\atlantis\report-3");
            // var input = @"* Unit m2 (2530), Avalon Empire (15), avoiding, wood elf [WELF], horse [HORS]. Weight: 60. Capacity: 0/70/85/0. Skills: combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).
// ";
/*
* Unit m2 (2530), Avalon Empire (15), avoiding, wood elf [WELF], horse [HORS]. Weight: 60. Capacity: 0/70/85/0. Skills: combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).
*/

            Result<char, IReportNode> result = null;
            result = AtlantisParser.Report.Parse(input);
            AssertParsed(result);


            if (result.Success) {
                var report = result.Value;

                Console.WriteLine(report.ToString());
                Console.WriteLine();
                Console.WriteLine("OK");

                var regions = report.ByType("regions");
                if (regions.Length > 1) {
                    Console.WriteLine($"Multiple 'regions' nodes - {regions.Length}");
                }
            }

        }

        public static void AssertParsed<T>(Result<char, T> res) {
            if (!res.Success) {
                if (!string.IsNullOrWhiteSpace(res.Error.Message)) {
                    Console.WriteLine(res.Error.Message);
                }

                if (res.Error.EOF) {
                    Console.WriteLine("Reached EOF");
                }

                var unex = res.Error.Unexpected;
                var pos = res.Error.ErrorPos;

                Console.WriteLine($"Error at (Ln {pos.Line}, Col {pos.Col})");

                foreach (var e in res.Error.Expected) {
                    var tokens = e.Tokens ?? Enumerable.Empty<char>();
                    var expected = tokens.Any()
                        ? string.Join("", tokens)
                        : e.Label;

                    Console.WriteLine($"Expected \"{expected}\"");
                }

                if (unex.HasValue) {
                    Console.WriteLine($"Found \"{unex.Value}\"");
                }
            }
        }
    }
}
