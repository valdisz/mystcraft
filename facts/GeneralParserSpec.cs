namespace atlantis.facts {
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using FluentAssertions;
    using Pidgin;
    using Xunit;
    using Xunit.Abstractions;

    public class GeneralParserSpec {
        public GeneralParserSpec(ITestOutputHelper output) {
            this.output = output;
        }

        private readonly ITestOutputHelper output;

        [Theory]
        [InlineData("War 1", "War", 1)]
        [InlineData("Trade 1", "Trade", 1)]
        [InlineData("Magic 1", "Magic", 1)]
        [InlineData("Ocean Fleets 5", "Ocean Fleets", 5)]
        public void FactionAttribute(string input, string key, int value) {
            var result = AtlantisParser.FactionAttribute.Parse(input);
            output.AssertParsed(result);

            (result.Value.Children[0] as ValueReportNode<string>).Value.Should().Be(key);
            (result.Value.Children[1] as ValueReportNode<int>).Value.Should().Be(value);
        }

        [Theory]
        [InlineData("(War 1, Trade 2, Magic 2)", 3)]
        [InlineData("(War 1,\n  Trade 2, Magic 2)", 3)]
        [InlineData("(War 1, Trade 2,\n  Magic 2)", 3)]
        public void FactionAttributes(string input, int count) {
            var result = AtlantisParser.FactionAttributes.Parse(input);
            output.AssertParsed(result);

            result.Value.Children.Count.Should().Be(count);
        }

        [Fact]
        public void FactionNumber() {
            var result = AtlantisParser.FactionNumber.Parse("(15)");
            output.AssertParsed(result);

            (result.Value as ValueReportNode<int>).Value.Should().Be(15);
        }

        [Theory]
        [InlineData("Avalon Empire (15)", "Avalon Empire", 15)]
        [InlineData("Avalon Empire  (15)", "Avalon Empire", 15)]
        [InlineData("Avalon Empire   (15)", "Avalon Empire", 15)]
        [InlineData("Avalon\n  Empire (15)", "Avalon Empire", 15)]
        [InlineData("Avalon Empire\n  (15)", "Avalon Empire", 15)]
        public void FactionNameAndNumber(string input, string name, int number) {
            var result = AtlantisParser.Faction.Parse(input);
            output.AssertParsed(result);

            var faction = result.Value;
            faction.StrValueOf("name").Should().Be(name);
            faction.IntValueOf("number").Should().Be(number);
        }

        [Theory]
        [InlineData("Avalon Empire (15) (War 1, Trade 2, Magic 2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon Empire (15) (War 1, Trade 2, Magic\n  2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon Empire (15) (War 1, Trade 2,\n  Magic 2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon Empire (15) (War 1, Trade\n  2, Magic 2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon Empire (15) (War 1,\n  Trade 2, Magic 2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon Empire (15) (War\n  1, Trade 2, Magic 2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon Empire (15)\n  (War 1, Trade 2, Magic 2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon Empire\n  (15) (War 1, Trade 2, Magic 2)", "Avalon Empire", 15, 3)]
        [InlineData("Avalon\n  Empire (15) (War 1, Trade 2, Magic 2)", "Avalon Empire", 15, 3)]
        public void ReportFaction(string input, string name, int number, int count) {
            var result = AtlantisParser.ReportFaction.Parse(input);
            output.AssertParsed(result);

            var faction = result.Value;

            faction.FirstByType("faction")?.StrValueOf("name").Should().Be(name);
            faction.FirstByType("faction")?.IntValueOf("number").Should().Be(number);
            faction.FirstByType("attributes")?.Children.Count.Should().Be(count);
        }

        [Theory]
        [InlineData("May, Year 3", "May", 3)]
        [InlineData("February, Year 1", "February", 1)]
        [InlineData("February, Year\n  1", "February", 1)]
        [InlineData("February,\n  Year 1", "February", 1)]
        public void Date(string input, string month, int year) {
            var result = AtlantisParser.ReportDate.Parse(input);
            output.AssertParsed(result);

            var node = result.Value;
            (node.Children[0] as ValueReportNode<string>).Value.Should().Be(month);
            (node.Children[1] as ValueReportNode<int>).Value.Should().Be(year);
        }

        [Theory]
        [InlineData("(0,0,2 <underworld>)", 0, 0, 2, "underworld")]
        [InlineData("(0,0,2\n  <underworld>)", 0, 0, 2, "underworld")]
        [InlineData("(1,1)", 1, 1, null, null)]
        public void Coords(string input, int x, int y, int? z, string label) {
            var result = AtlantisParser.Coords.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
            v.IntValueOf("x").Should().Be(x);
            v.IntValueOf("y").Should().Be(y);
            v.IntValueOf("z").Should().Be(z);
            v.StrValueOf("label").Should().Be(label);
        }

        [Theory]
        [InlineData("contains Sinsto [town]", "Sinsto", "town")]
        public void Settlement(string input, string name, string size) {
            var result = AtlantisParser.Settlement.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
            v.StrValueOf("name").Should().Be(name);
            v.StrValueOf("size").Should().Be(size);
        }

        [Theory]
        [InlineData("1195 peasants (gnomes)", 1195, "gnomes")]
        [InlineData("10237 peasants (drow elves)", 10237, "drow elves")]
        [InlineData("10237 peasants (drow\n  elves)", 10237, "drow elves")]
        [InlineData("10237 peasants\n  (drow elves)", 10237, "drow elves")]
        [InlineData("10237\n  peasants (drow elves)", 10237, "drow elves")]
        public void Population(string input, int amount, string race) {
            var result = AtlantisParser.Population.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
            v.IntValueOf("amount").Should().Be(amount);
            v.StrValueOf("race").Should().Be(race);
        }

        [Theory]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, 100 peasants (humans).\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, 100 peasants\n  (humans).\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, 100\n  peasants (humans).\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway,\n  100 peasants (humans).\n")]
        [InlineData("underforest (50,0,2 <underworld>) in\n  Ryway, 100 peasants (humans).\n")]
        [InlineData("underforest (50,0,2 <underworld>)\n  in Ryway, 100 peasants (humans).\n")]
        [InlineData("underforest (50,0,2\n  <underworld>) in Ryway, 100 peasants (humans).\n")]
        [InlineData("underforest\n  (50,0,2 <underworld>) in Ryway, 100 peasants (humans).\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town], 10237 peasants (drow elves), $5937.\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town],\n  10237 peasants (drow elves), $5937.\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, contains Sinsto\n  [town], 10237 peasants (drow elves), $5937.\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, contains\n  Sinsto [town], 10237 peasants (drow elves), $5937.\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway,\n  contains Sinsto [town], 10237 peasants (drow elves), $5937.\n")]
        [InlineData("underforest (52,0,2 <underworld>) in Ryway, 1872 peasants (drow elves), $1385.\n")]
        public void RegionSummary(string input) {
            var result = AtlantisParser.RegionHeader.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
        }

        [Theory]
        [InlineData("The weather was clear last month; it will be clear next month.", null)]
        [InlineData("Wages: $16.9 (Max: $5331).", "Wages")]
        [InlineData("Wanted: none.", "Wanted")]
        [InlineData("For Sale: 65 orcs [ORC] at $42, 13 leaders [LEAD] at $744.", "For Sale")]
        [InlineData("Entertainment available: $54.", "Entertainment available")]
        [InlineData("Products: 15 grain [GRAI].", "Products")]
        public void RegionParam(string input, string capture) {
            var result = AtlantisParser.RegionAttribute.Parse(input);

            if (capture == null) {
                result.Success.Should().BeFalse();
                return;
            }

            output.AssertParsed(result);
            result.Value.Should().Be(capture);
        }

        [Fact]
        public void RegionParams() {
            var input = @"------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $13.3 (Max: $466).
  Wanted: none.
  For Sale: 65 orcs [ORC] at $42, 13 leaders [LEAD] at $744.
  Entertainment available: $54.
  Products: 15 grain [GRAI].

";
            var result = AtlantisParser.RegionAttributes.Parse(input);

            output.AssertParsed(result);

            output.WriteLine(result.Value.ToString());

            result.Value.StrValueOf("unknown").Should().Be("The weather was clear last month; it will be clear next month.");
            result.Value.FirstByType("wages")?.RealValueOf("salary").Should().Be(13.3);
            result.Value.FirstByType("wages")?.IntValueOf("max-wages").Should().Be(466);
            result.Value.FirstByType("wanted")?.Children.Count.Should().Be(0);
            result.Value.FirstByType("for-sale")?.Children?.Count.Should().Be(2);
            result.Value.IntValueOf("entertainment-available").Should().Be(54);
            result.Value.FirstByType("products")?.Children?.Count.Should().Be(1);
        }

        [Theory]
        [InlineData("orc [ORC] at $42", 1, "orc", "ORC", 42)]
        [InlineData("orc [ORC] at\n  $42", 1, "orc", "ORC", 42)]
        [InlineData("orc [ORC]\n  at $42", 1, "orc", "ORC", 42)]
        [InlineData("orc\n  [ORC] at $42", 1, "orc", "ORC", 42)]
        [InlineData("65 orcs [ORC] at $42", 65, "orcs", "ORC", 42)]
        [InlineData("65\n  orcs [ORC] at $42", 65, "orcs", "ORC", 42)]
        [InlineData("65 orcs [ORC]", 65, "orcs", "ORC", null)]
        [InlineData("orc [ORC]", 1, "orc", "ORC", null)]
        public void Item(string input, int amount, string name, string code, int? price) {
            var result = AtlantisParser.Item.Parse(input);
            output.AssertParsed(result);

            var value = result.Value;
            value.IntValueOf("amount").Should().Be(amount);
            value.StrValueOf("name").Should().Be(name);
            value.StrValueOf("code").Should().Be(code);
            value.IntValueOf("price").Should().Be(price);
        }

        [Theory]
        [InlineData(@"underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town],
  10237 peasants (drow elves), $5937.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $12.9 (Max: $1187).
  Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123
    fish [FISH] at $27, 7 leather armor [LARM] at $69.
  For Sale: 409 drow elves [DRLF] at $41, 81 leaders [LEAD] at $722.
  Entertainment available: $399.
  Products: 17 livestock [LIVE], 12 wood [WOOD], 15 stone [STON], 12
    iron [IRON].

Exits:
  Southeast : underforest (51,1,2 <underworld>) in Ryway.
  South : underforest (50,2,2 <underworld>) in Ryway.
  Southwest : underforest (49,1,2 <underworld>) in Hawheci.

- Unit (2465), Semigallians (18), avoiding, behind, under dwarf
  [UDWA].
- Scout (2070), Disasters Inc (43), avoiding, behind, revealing
  faction, receiving no aid, high elf [HELF], horse [HORS], 40 silver
  [SILV]. Weight: 60. Capacity: 0/70/85/0. Skills: riding [RIDI] 1
  (60).
- tactician (5246), Anuii (44), revealing faction, holding, weightless
  battle spoils, drow elf [DRLF], 6 silver [SILV]. Weight: 10.
  Capacity: 0/0/15/0. Skills: tactics [TACT] 2 (90).
- taxmen (5600), Anuii (44), revealing faction, holding, sharing, 2
  under dwarves [UDWA], 118 drow elves [DRLF], horse [HORS], 14241
  silver [SILV]. Weight: 1250. Capacity: 0/70/1870/0. Skills: combat
  [COMB] 1 (35).
- Ambassador (5279), Noizy Tribe (49), avoiding, behind, orc [ORC],
  horse [HORS].
- Unit (9349), Anuii (44), avoiding, behind, revealing faction,
  holding, weightless battle spoils, under dwarf [UDWA], 2 scrying
  orbs [SORB], 9734 silver [SILV]. Weight: 10. Capacity: 0/0/15/0.
  Skills: none.
- b gemcutting (6366), Disasters Inc (43), avoiding, behind, revealing
  faction, 10 under dwarves [UDWA], 5 horses [HORS], 300 silver
  [SILV]. Weight: 350. Capacity: 0/350/500/0. Skills: gemcutting
  [GCUT] 3 (180).

+ Shaft [1] : Shaft, contains an inner location.
  - scout (4637), Anuii (44), avoiding, behind, revealing faction,
    receiving no aid, sharing, weightless battle spoils, under dwarf
    [UDWA], horse [HORS], 52 silver [SILV]. Weight: 60. Capacity:
    0/70/85/0. Skills: none.

")]
        [InlineData(@"forest (50,22) in Mapa, contains Sembury [village], 5866 peasants
  (high elves), $2698.
------------------------------------------------------------
  Wages: $12.3 (Max: $539).
  Wanted: 96 grain [GRAI] at $21, 112 livestock [LIVE] at $19, 86 fish
    [FISH] at $23.
  For Sale: 234 high elves [HELF] at $39, 46 leaders [LEAD] at $688.
  Entertainment available: $193.
  Products: 32 grain [GRAI], 24 wood [WOOD], 11 furs [FUR], 16 herbs
    [HERB].

Exits:
  North : forest (50,20) in Mapa.
  Northeast : plain (51,21) in Inthon.
  Southeast : plain (51,23) in Inthon.
  South : ocean (50,24) in Atlantis Ocean.
  Southwest : ocean (49,23) in Atlantis Ocean.
  Northwest : forest (49,21) in Mapa.

- City Guard (35), on guard, The Guardsmen (1), 40 leaders [LEAD], 40
  swords [SWOR].
* Emperor (456), Avalon Empire (15), avoiding, behind, holding, won't
  cross water, leader [LEAD]. Weight: 10. Capacity: 0/0/15/0. Skills:
  force [FORC] 1 (30), pattern [PATT] 1 (30), spirit [SPIR] 1 (30),
  gate lore [GATE] 1 (30), combat [COMB] 3 (180), endurance [ENDU] 3
  (180). Can Study: fire [FIRE], earthquake [EQUA], force shield
  [FSHI], energy shield [ESHI], spirit shield [SSHI], magical healing
  [MHEA], farsight [FARS], mind reading [MIND], weather lore [WEAT],
  earth lore [EART], necromancy [NECR], demon lore [DEMO], illusion
  [ILLU], artifact lore [ARTI].

")]
        [InlineData(@"mountain (10,2) in Metfle, 2021 peasants (orcs), $889.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $12.2 (Max: $370).
  Wanted: none.
  For Sale: 80 orcs [ORC] at $39, 16 leaders [LEAD] at $683.
  Entertainment available: $53.
  Products: 28 livestock [LIVE], 38 iron [IRON], 11 stone [STON], 6
    mithril [MITH].

Exits:
  North : ocean (10,0) in Atlantis Ocean.
  Northeast : ocean (11,1) in Atlantis Ocean.
  Southeast : mountain (11,3) in Metfle.
  South : mountain (10,4) in Metfle.
  Southwest : ocean (9,3) in Atlantis Ocean.
  Northwest : ocean (9,1) in Atlantis Ocean.

- p iron (5220), Anuii (44), avoiding, behind, revealing faction,
  sharing, weightless battle spoils, 6 humans [MAN], 6 picks [PICK],
  52 iron [IRON]. Weight: 326. Capacity: 0/0/90/0. Skills: mining
  [MINI] 3 (260), riding [RIDI] 1 (30).
- p mith (5561), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 2 humans [MAN], 12 mithril [MITH]. Weight:
  140. Capacity: 0/0/30/0. Skills: mining [MINI] 3 (260), riding
  [RIDI] 1 (30).
- p weap-quar (7637), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 12 orcs [ORC], 16 hammers [HAMM], 11 stone
  [STON], 1028 silver [SILV]. Weight: 686. Capacity: 0/0/180/0.
  Skills: weaponsmith [WEAP] 2 (90), quarrying [QUAR] 1 (40).
- miner (2898), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 2 humans [MAN], 2 picks [PICK], 20 iron
  [IRON]. Weight: 122. Capacity: 0/0/30/0. Skills: mining [MINI] 4
  (300), riding [RIDI] 1 (30).
- Jester (5626), The Lizards (20), avoiding, behind, orc [ORC].
- Lizardmen (6656), The Lizards (20), avoiding, behind, lizardman
  [LIZA].
- taxmen (5923), Anuii (44), revealing faction, holding, 8 wood elves
  [WELF], 378 silver [SILV]. Weight: 80. Capacity: 0/0/120/0. Skills:
  combat [COMB] 1 (30).
- scout (2907), on guard, Anuii (44), behind, revealing faction,
  holding, weightless battle spoils, centaur [CTAU], 48 silver [SILV].
  Weight: 50. Capacity: 0/70/70/0. Skills: riding [RIDI] 1 (55).
- scout (4611), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, orc [ORC], 2 silver [SILV]. Weight: 10.
  Capacity: 0/0/15/0. Skills: none.

+ Shaft [1] : Shaft, contains an inner location.

+ Fleet [177] : Cog; Load: 20/500; Sailors: 6/6; MaxSpeed: 4.
  - captain cog 177 (6792), Anuii (44), avoiding, behind, revealing
    faction, weightless battle spoils, 2 lizardmen [LIZA], 980 silver
    [SILV]. Weight: 20. Capacity: 0/0/30/30. Skills: sailing [SAIL] 3
    (205).
")]
        public void Region(string input) {
            var result = AtlantisParser.Region.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
        }

        [Theory]
        [InlineData(@"mountain (10,2) in Metfle, 2021 peasants (orcs), $889.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $12.2 (Max: $370).
  Wanted: none.
  For Sale: 80 orcs [ORC] at $39, 16 leaders [LEAD] at $683.
  Entertainment available: $53.
  Products: 28 livestock [LIVE], 38 iron [IRON], 11 stone [STON], 6
    mithril [MITH].

Exits:
  North : ocean (10,0) in Atlantis Ocean.
  Northeast : ocean (11,1) in Atlantis Ocean.
  Southeast : mountain (11,3) in Metfle.
  South : mountain (10,4) in Metfle.
  Southwest : ocean (9,3) in Atlantis Ocean.
  Northwest : ocean (9,1) in Atlantis Ocean.

- p iron (5220), Anuii (44), avoiding, behind, revealing faction,
  sharing, weightless battle spoils, 6 humans [MAN], 6 picks [PICK],
  52 iron [IRON]. Weight: 326. Capacity: 0/0/90/0. Skills: mining
  [MINI] 3 (260), riding [RIDI] 1 (30).
- p mith (5561), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 2 humans [MAN], 12 mithril [MITH]. Weight:
  140. Capacity: 0/0/30/0. Skills: mining [MINI] 3 (260), riding
  [RIDI] 1 (30).
- p weap-quar (7637), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 12 orcs [ORC], 16 hammers [HAMM], 11 stone
  [STON], 1028 silver [SILV]. Weight: 686. Capacity: 0/0/180/0.
  Skills: weaponsmith [WEAP] 2 (90), quarrying [QUAR] 1 (40).
- miner (2898), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 2 humans [MAN], 2 picks [PICK], 20 iron
  [IRON]. Weight: 122. Capacity: 0/0/30/0. Skills: mining [MINI] 4
  (300), riding [RIDI] 1 (30).
- Jester (5626), The Lizards (20), avoiding, behind, orc [ORC].
- Lizardmen (6656), The Lizards (20), avoiding, behind, lizardman
  [LIZA].
- taxmen (5923), Anuii (44), revealing faction, holding, 8 wood elves
  [WELF], 378 silver [SILV]. Weight: 80. Capacity: 0/0/120/0. Skills:
  combat [COMB] 1 (30).
- scout (2907), on guard, Anuii (44), behind, revealing faction,
  holding, weightless battle spoils, centaur [CTAU], 48 silver [SILV].
  Weight: 50. Capacity: 0/70/70/0. Skills: riding [RIDI] 1 (55).
- scout (4611), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, orc [ORC], 2 silver [SILV]. Weight: 10.
  Capacity: 0/0/15/0. Skills: none.

+ Shaft [1] : Shaft, contains an inner location.

+ Fleet [177] : Cog; Load: 20/500; Sailors: 6/6; MaxSpeed: 4.
  - captain cog 177 (6792), Anuii (44), avoiding, behind, revealing
    faction, weightless battle spoils, 2 lizardmen [LIZA], 980 silver
    [SILV]. Weight: 20. Capacity: 0/0/30/30. Skills: sailing [SAIL] 3
    (205).


swamp (40,2) in Lihes, contains Panedyl [village], 5835 peasants
  (lizardmen), $1750.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $11.5 (Max: $350).
  Wanted: 122 grain [GRAI] at $18, 114 livestock [LIVE] at $23, 129
    fish [FISH] at $21.
  For Sale: 233 lizardmen [LIZA] at $36, 46 leaders [LEAD] at $644.
  Entertainment available: $145.
  Products: 17 livestock [LIVE], 16 wood [WOOD], 19 herbs [HERB].

Exits:
  North : swamp (40,0) in Lihes.
  Northeast : swamp (41,1) in Lihes.
  Southeast : swamp (41,3) in Lihes.
  South : swamp (40,4) in Lihes.
  Southwest : swamp (39,3) in Lihes.
  Northwest : swamp (39,1) in Lihes.

- Marines (1416), Noizy Tribe (49), 17 lizardmen [LIZA], 3 spears
  [SPEA], 17 livestock [LIVE].
- City Guard (2966), on guard, The Guardsmen (1), 40 leaders [LEAD],
  40 swords [SWOR].
- Weaponsmiths (2370), Noizy Tribe (49), avoiding, behind, 3 goblins
  [GBLN], 2 lizardmen [LIZA], 2 axes [AXE], 62 crossbows [XBOW].
- Scout (2258), Silver Hand (27), avoiding, behind, goblin [GBLN].
- choppers (6852), Noizy Tribe (49), avoiding, behind, 7 lizardmen
  [LIZA].
- Ambassador Whereitallbegan (4290), Disasters Inc (43), avoiding,
  behind, revealing faction, lizardman [LIZA], 6 silver [SILV].
  Weight: 10. Capacity: 0/0/15/15. Skills: none.
- ranger (8928), Noizy Tribe (49), leader [LEAD].
- scout (9371), Noizy Tribe (49), behind, 2 gnomes [GNOM], winged
  horse [WING].
", 2)]
        [InlineData(@"underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town],
  10237 peasants (drow elves), $5937.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $12.9 (Max: $1187).
  Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123
    fish [FISH] at $27, 7 leather armor [LARM] at $69.
  For Sale: 409 drow elves [DRLF] at $41, 81 leaders [LEAD] at $722.
  Entertainment available: $399.
  Products: 17 livestock [LIVE], 12 wood [WOOD], 15 stone [STON], 12
    iron [IRON].

Exits:
  Southeast : underforest (51,1,2 <underworld>) in Ryway.
  South : underforest (50,2,2 <underworld>) in Ryway.
  Southwest : underforest (49,1,2 <underworld>) in Hawheci.

- Unit (2465), Semigallians (18), avoiding, behind, under dwarf
  [UDWA].
- Scout (2070), Disasters Inc (43), avoiding, behind, revealing
  faction, receiving no aid, high elf [HELF], horse [HORS], 40 silver
  [SILV]. Weight: 60. Capacity: 0/70/85/0. Skills: riding [RIDI] 1
  (60).
- tactician (5246), Anuii (44), revealing faction, holding, weightless
  battle spoils, drow elf [DRLF], 6 silver [SILV]. Weight: 10.
  Capacity: 0/0/15/0. Skills: tactics [TACT] 2 (90).
- taxmen (5600), Anuii (44), revealing faction, holding, sharing, 2
  under dwarves [UDWA], 118 drow elves [DRLF], horse [HORS], 14241
  silver [SILV]. Weight: 1250. Capacity: 0/70/1870/0. Skills: combat
  [COMB] 1 (35).
- Ambassador (5279), Noizy Tribe (49), avoiding, behind, orc [ORC],
  horse [HORS].
- Unit (9349), Anuii (44), avoiding, behind, revealing faction,
  holding, weightless battle spoils, under dwarf [UDWA], 2 scrying
  orbs [SORB], 9734 silver [SILV]. Weight: 10. Capacity: 0/0/15/0.
  Skills: none.
- b gemcutting (6366), Disasters Inc (43), avoiding, behind, revealing
  faction, 10 under dwarves [UDWA], 5 horses [HORS], 300 silver
  [SILV]. Weight: 350. Capacity: 0/350/500/0. Skills: gemcutting
  [GCUT] 3 (180).

+ Shaft [1] : Shaft, contains an inner location.
  - scout (4637), Anuii (44), avoiding, behind, revealing faction,
    receiving no aid, sharing, weightless battle spoils, under dwarf
    [UDWA], horse [HORS], 52 silver [SILV]. Weight: 60. Capacity:
    0/70/85/0. Skills: none.


underforest (52,0,2 <underworld>) in Ryway, 1872 peasants (drow
  elves), $1385.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $13.7 (Max: $581).
  Wanted: none.
  For Sale: 74 drow elves [DRLF] at $43, 14 leaders [LEAD] at $767.
  Entertainment available: $71.
  Products: 14 grain [GRAI], 10 wood [WOOD], 14 stone [STON], 19 iron
    [IRON].

Exits:
  Southeast : underforest (53,1,2 <underworld>) in Ryway.
  South : underforest (52,2,2 <underworld>) in Ryway.

- stow guard (7622), Disasters Inc (43), avoiding, behind, revealing
  faction, holding, receiving no aid, weightless battle spoils, under
  dwarf [UDWA], 19 silver [SILV]. Weight: 10. Capacity: 0/0/15/0.
  Skills: none.
- Unit (9282), Disasters Inc (43), avoiding, behind, revealing
  faction, sharing, hill dwarf [HDWA], winged horse [WING], 90 silver
  [SILV]. Weight: 60. Capacity: 70/70/85/0. Skills: mining [MINI] 5
  (450).

underforest (54,0,2 <underworld>) in Ryway, 1618 peasants (humans),
  $970.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $13.0 (Max: $421).
  Wanted: none.
  For Sale: 64 humans [MAN] at $41, 12 leaders [LEAD] at $728.
  Entertainment available: $49.
  Products: 10 grain [GRAI], 13 wood [WOOD], 13 stone [STON], 18 iron
    [IRON].

Exits:
  Southeast : underforest (55,1,2 <underworld>) in Ryway.
  South : underforest (54,2,2 <underworld>) in Ryway, contains
    Stowotpest [town].

- trigger (4327), Anuii (44), avoiding, behind, revealing faction,
  receiving no aid, weightless battle spoils, under dwarf [UDWA], 39
  silver [SILV]. Weight: 10. Capacity: 0/0/15/0. Skills: none.
- stow guard (7627), Disasters Inc (43), behind, revealing faction,
  holding, receiving no aid, weightless battle spoils, under dwarf
  [UDWA], 32 silver [SILV]. Weight: 10. Capacity: 0/0/15/0. Skills:
  none.

", 3)]
        public void canParseRegions(string input, int regionCount) {
            var result = AtlantisParser.Regions.Parse(input);
            (output.AssertParsed(result).Value?.Children?.Count ?? 0).Should().Be(regionCount);
        }

        [Theory]
        [InlineData("Atlantis Engine Version: 5.2.0 (beta)\n")]
        [InlineData("NewOrigins, Version: 1.0.1 (beta)\n")]
        [InlineData("Faction Status:\n")]
        [InlineData("Tax Regions: 0 (15)\n")]
        [InlineData("Trade Regions: 0 (15)\n")]
        [InlineData("Mages: 1 (2)\n")]
        [InlineData("Apprentices: 0 (3)\n")]
        [InlineData("Events during turn:\n")]
        [InlineData("Emperor (456): Claims $50.\n")]
        [InlineData("Emperor (456): Enters Gateway to forest [2].\n")]
        [InlineData(@"Emperor (456): Walks from nexus (0,0,0 <nexus>) in The Void to forest
  (50,22) in Mapa.
")]
        [InlineData("Item reports:\n")]
        [InlineData("silver [SILV], weight 0. This is the currency of Atlantis.\n")]
        [InlineData("Declared Attitudes (default Neutral):\n")]
        [InlineData("Hostile : none.\n")]
        [InlineData("Unfriendly : Creatures (2).\n")]
        [InlineData("Neutral : none.\n")]
        [InlineData("Friendly : none.\n")]
        [InlineData("Ally : none.\n")]
        [InlineData("Unclaimed silver: 10000.\n")]
        [InlineData("\n")]
        public void ReportLine(string input) {
            var result = AtlantisParser.ReportLine.Parse(input);
            output.AssertParsed(result);
        }

        [Theory]
        [InlineData("+ Fleet [128] : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [128] : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed:\n  4.\n")]
        [InlineData("+ Fleet [128] : Longship; Load: 93/100; Sailors: 4/4;\n  MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [128] : Longship; Load: 93/100; Sailors:\n  4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [128] : Longship; Load: 93/100;\n  Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [128] : Longship; Load:\n  93/100; Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [128] : Longship;\n  Load: 93/100; Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [128] :\n  Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [128]\n  : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet\n  [128] : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData("+ Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors:\n  10/10; MaxSpeed: 4.\n")]
        [InlineData("+ Shaft [1] : Shaft, contains an inner location.\n")]
        [InlineData("+ AE Sembury [165] : Cog; Load: 500/500; Sailors: 6/6; MaxSpeed: 4; Imperial Trade Fleet.")]
        public void canParseStructure(string input) {
            var result = AtlantisParser.Structure.Parse(input);
            output.AssertParsed(result);
        }

        [Theory]
        [InlineData(@"+ Fleet [128] : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.
  - captain longship 128 (3941), Anuii (44), avoiding, behind,
    revealing faction, weightless battle spoils, 2 gnomes [GNOM], 83
    crossbows [XBOW], 36 herbs [HERB]. Weight: 93. Capacity: 0/0/18/0.
    Skills: sailing [SAIL] 2 (90).
", 1)]
        [InlineData(@"+ Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors:
  10/10; MaxSpeed: 4.
  - captain fleet 121 (3940), Anuii (44), avoiding, behind, revealing
    faction, weightless battle spoils, 2 gnomes [GNOM], 11 stone
    [STON]. Weight: 560. Capacity: 0/0/18/0. Skills: sailing [SAIL] 2
    (90).
  - Unit (6795), Anuii (44), avoiding, behind, revealing faction,
    weightless battle spoils, 2 lizardmen [LIZA]. Weight: 20.
    Capacity: 0/0/30/30. Skills: sailing [SAIL] 3 (195).
", 2)]
        [InlineData(@"+ Building [1] : Fort.
  - Unit (469), Demon's Duchy (28), behind, leader [LEAD].
", 1)]
        public void canPraseStructureWithUnits(string input, int units) {
            var result = AtlantisParser.StructureWithUnits.Parse(input);
            output.AssertParsed(result);

            var structure = result.Value;
            output.WriteLine(structure.ToString());

            (structure.FirstByType("units")?.Children?.Count ?? 0).Should().Be(units);
        }

        [Theory]
        [InlineData(@"  - Unit (6795), Anuii (44), avoiding, behind, revealing faction,
    weightless battle spoils, 2 lizardmen [LIZA]. Weight: 20.
    Capacity: 0/0/30/30. Skills: sailing [SAIL] 3 (195).
")]
        public void canParseUnitWithlargerIdent(string input) {
            var result = AtlantisParser.Unit(Parser.Char(' ').Repeat(2).IgnoreResult()).Parse(input);
            output.AssertParsed(result);
        }

        [Theory]
        [InlineData(@"  - captain fleet 121 (3940), Anuii (44), avoiding, behind, revealing
    faction, weightless battle spoils, 2 gnomes [GNOM], 11 stone
    [STON]. Weight: 560. Capacity: 0/0/18/0. Skills: sailing [SAIL] 2
    (90).
  - Unit (6795), Anuii (44), avoiding, behind, revealing faction,
    weightless battle spoils, 2 lizardmen [LIZA]. Weight: 20.
    Capacity: 0/0/30/30. Skills: sailing [SAIL] 3 (195).
", 2)]
        public void canParseMultipleUnitsWithlargerIdent(string input, int unitCount) {
            var result = AtlantisParser.Units(Parser.Char(' ').Repeat(2).IgnoreResult()).Parse(input);
            output.AssertParsed(result).Value?.Children?.Count.Should().Be(unitCount);
        }

        [Theory]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross water, wood elf [WELF], horse\n  [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross water, wood elf [WELF],\n  horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross water, wood elf\n  [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross water, wood\n  elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross water,\n  wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross\n  water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't\n  cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid,\n  won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no\n  aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving\n  no aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction, holding,\n  receiving no aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing faction,\n  holding, receiving no aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind, revealing\n  faction, holding, receiving no aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding, behind,\n  revealing faction, holding, receiving no aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        [InlineData("avoiding,\n  behind, revealing faction, holding, receiving no aid, won't cross water, wood elf [WELF], horse [HORS].", 6, 2)]
        public void canParseUnitFlagsAndSkills(string input, int falgsCount, int itemCount) {
            var result = AtlantisParser.UnitFlagsAndItems.Parse(input);
            output.AssertParsed(result);

            var (flags, items) = result.Value;
            (flags?.Children?.Count ?? 0).Should().Be(falgsCount);
            (items?.Children?.Count ?? 0).Should().Be(itemCount);
        }

        [Theory]
        [InlineData(". Weight: 60.\n", 1)]  // attribute is final in unit
        [InlineData(". Weight: 60;", 1)]    // after attrbute will be description
        [InlineData(". Capacity: 0/70/85/0.\n", 1)]
        [InlineData(". Capacity: 0/70/85/0;", 1)]
        [InlineData(". Weight: 60. Capacity: 0/70/85/0.\n", 2)]
        [InlineData(". Weight: 60. Capacity: 0/70/85/0;", 2)]
        [InlineData(". Skills: combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).\n", 1)]
        [InlineData(". Skills: combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65);", 1)]
        [InlineData(". Weight: 60. Capacity: 0/70/85/0. Skills: combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).\n", 3)]
        [InlineData(". Weight: 60. Capacity: 0/70/85/0. Skills: combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65);", 3)]
        public void unitAttributes(string input, int count) {
            var result = AtlantisParser.UnitAttributes().Parse(input);
            output.AssertParsed(result);

            (result.Value?.Children?.Count ?? 0).Should().Be(count);
        }

        [Fact]
        public void canParserUnit() {
            var result = AtlantisParser.Unit().Parse(@"* Unit m2 (2530), Avalon Empire (15), avoiding, behind, revealing
  faction, holding, receiving no aid, won't cross water, wood elf
  [WELF], horse [HORS]. Weight: 60. Capacity: 0/70/85/0. Skills:
  combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).
");
            output.AssertParsed(result);

            output.WriteLine(result.Value.ToString());
        }

        [Theory]
        [InlineData(@"- Dwarven masons (3860), 3 hill dwarves [HDWA], 3 picks [PICK], 41
  stone [STON]; They are dusty and brawny..
", "They are dusty and brawny.")]
        [InlineData(@"- Herdsmen of the Ungre Guild (1767), 3 humans [MAN], 3 spices [SPIC],
  2 gems [GEM], net [NET], 2 mink [MINK], 3 livestock [LIVE]; Content
  looking shepherds and herdsmen.
", "Content looking shepherds and herdsmen")]
        public void canParseUnitWithDescription(string input, string description) {
            var result = AtlantisParser.Unit().Parse(input);
            var unit = output.AssertParsed(result).Value;

            unit.StrValueOf("description").Should().Be(description);
        }

        [Theory]
        [InlineData(".\n", true)]
        [InlineData(".\n  foo", false)]
        [InlineData(".\nfoo", true)]
        public void unitTerminator(string input, bool success) {
            var result = AtlantisParser.UnitTerminator().Parse(input);
            result.Success.Should().Be(success);
        }

        [Fact]
        public void canParserUnits() {
            var result = AtlantisParser.Units().Parse(@"* Unit m2 (2530), Avalon Empire (15), avoiding, behind, revealing
  faction, holding, receiving no aid, won't cross water, wood elf
  [WELF], horse [HORS]. Weight: 60. Capacity: 0/70/85/0. Skills:
  combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).
- Aquatic Scout (3427), Disasters Inc (43), avoiding, behind,
  revealing faction, lizardman [LIZA]. Weight: 10. Capacity:
  0/0/15/15. Skills: none.
- Scout (2070), Disasters Inc (43), avoiding, behind, revealing
  faction, receiving no aid, high elf [HELF], horse [HORS], 40 silver
  [SILV]. Weight: 60. Capacity: 0/70/85/0. Skills: riding [RIDI] 1
  (60).
");
            output.AssertParsed(result);

            output.WriteLine(result.Value.ToString());
        }

        [Theory]
        [InlineData("foo, bar, wood elf [WELF]", 2)]
        [InlineData("foo, bar, 10 orcs [ORC]", 2)]
        [InlineData("avoiding, behind, revealing faction, holding, receiving no aid, won't cross water, 3 swords [SWOR]", 6)]
        public void unitFlags(string input, int count) {
            var result = AtlantisParser.UnitFlags.Parse(input);
            output.AssertParsed(result);

            result.Value.Children.Count.Should().Be(count);
        }
    }
}
