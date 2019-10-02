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
            var result = RegionParser.FactionAttribute.Parse(input);
            output.AssertParsed(result);

            (result.Value.Children[0] as ValueReportNode<string>).Value.Should().Be(key);
            (result.Value.Children[1] as ValueReportNode<int>).Value.Should().Be(value);
        }

        [Theory]
        [InlineData("(War 1, Trade 2, Magic 2)", 3)]
        public void FactionAttributes(string input, int count) {
            var result = RegionParser.FactionAttributes.Parse(input);
            output.AssertParsed(result);

            result.Value.Children.Count.Should().Be(count);
        }

        [Fact]
        public void FactionNumber() {
            var result = RegionParser.FactionNumber.Parse("(15)");
            output.AssertParsed(result);

            (result.Value as ValueReportNode<int>).Value.Should().Be(15);
        }

        [Fact]
        public void FactionInfo() {
            var result = RegionParser.ReportFaction.Parse("Avalon Empire (15) (War 1, Trade 2, Magic 2)");
            output.AssertParsed(result);
        }

        [Theory]
        [InlineData("May, Year 3", "May", 3)]
        [InlineData("February, Year 1", "February", 1)]
        public void Date(string input, string month, int year) {
            var result = RegionParser.ReportDate.Parse(input);
            output.AssertParsed(result);

            var node = result.Value;
            (node.Children[0] as ValueReportNode<string>).Value.Should().Be(month);
            (node.Children[1] as ValueReportNode<int>).Value.Should().Be(year);
        }

        [Theory]
        [InlineData("atlantis", true)]
        [InlineData("atlantis text", true)]
        [InlineData(" cannot start with space", false)]
        [InlineData("a", true)]
        public void AText(string input, bool success) {
            var result = Tokens.TText(Tokens.AtlantisCharset()).Parse(input);
            result.Success.Should().Be(success);
        }
    }

    public class RegionParserSpec {
        public RegionParserSpec(ITestOutputHelper output) {
            this.output = output;
        }

        private readonly ITestOutputHelper output;

        [Theory]
        [InlineData("(0,0,2 <underworld>)", 0, 0, 2, "underworld")]
        [InlineData("(1,1)", 1, 1, null, null)]
        public void Coords(string input, int x, int y, int? z, string label) {
            var result = RegionParser.Coords.Parse(input);
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
            var result = RegionParser.Settlement.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
            v.StrValueOf("name").Should().Be(name);
            v.StrValueOf("size").Should().Be(size);
        }

        [Theory]
        [InlineData("1195 peasants (gnomes)", 1195, "gnomes")]
        [InlineData("10237 peasants (drow elves)", 10237, "drow elves")]
        public void Population(string input, int amount, string race) {
            var result = RegionParser.Population.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
            v.IntValueOf("amount").Should().Be(amount);
            v.StrValueOf("race").Should().Be(race);
        }

        [Theory]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, 100 peasants (humans).\n")]
        [InlineData("underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town], 10237 peasants (drow elves), $5937.\n")]
        [InlineData(@"underforest (52,0,2 <underworld>) in Ryway, 1872 peasants (drow
  elves), $1385.
")]
        public void Summary(string input) {
            var result = RegionParser.Summary.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
        }

        [Theory]
        [InlineData("  Wages: $16.9 (Max: $5331).\n", "Wages: $16.9 (Max: $5331).")]
        // [InlineData("  The weather was clear last month; it will be clear next month.\n", "The weather was clear last month; it will be clear next month")]
        // [InlineData("  Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123\n    fish [FISH] at $27, 7 leather armor [LARM] at $69.\n", "Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123 fish [FISH] at $27, 7 leather armor [LARM] at $69")]
        public void RegionParam(string input, string capture) {
            var result = RegionParser.RegionAttributeLine.Parse(input);
            output.AssertParsed(result);

            result.Value.Should().Be(capture);
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
        public void Region(string input) {
            var result = RegionParser.Region.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
        }

        [Fact]
        public void Regions() {
            var result = RegionParser.Regions.Parse(@"underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town],
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

");
            output.AssertParsed(result);
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
            var result = RegionParser.ReportLine.Parse(input);
            output.AssertParsed(result);
        }
    }
}
