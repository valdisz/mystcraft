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

        [Fact]
        public void FactionInfo() {
            var result = AtlantisParser.ReportFaction.Parse("Avalon Empire (15) (War 1, Trade 2, Magic 2)");
            output.AssertParsed(result);
        }

        [Theory]
        [InlineData("May, Year 3", "May", 3)]
        [InlineData("February, Year 1", "February", 1)]
        public void Date(string input, string month, int year) {
            var result = AtlantisParser.ReportDate.Parse(input);
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
        public void Population(string input, int amount, string race) {
            var result = AtlantisParser.Population.Parse(input);
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
            var result = AtlantisParser.Summary.Parse(input);
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
        // [InlineData("  The weather was clear last month; it will be clear next month.\n", "The weather was clear last month; it will be clear next month")]
        // [InlineData("  Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123\n    fish [FISH] at $27, 7 leather armor [LARM] at $69.\n", "Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123 fish [FISH] at $27, 7 leather armor [LARM] at $69")]
        public void RegionParam(string input, string capture) {
            var result = AtlantisParser.RegionAttribute.Parse(input);

            if (capture == null) {
                result.Success.Should().BeFalse();
                return;
            }

            output.AssertParsed(result);
            result.Value.Should().Be(capture);
        }

        // [Fact]
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
            result.Value.StrValueOf("wages").Should().Be("$13.3 (Max: $466).");
            result.Value.StrValueOf("wanted").Should().Be("none.");
            result.Value.StrValueOf("for-sale").Should().Be("65 orcs [ORC] at $42, 13 leaders [LEAD] at $744.");
            result.Value.StrValueOf("entertainment-available").Should().Be("$54.");
            result.Value.StrValueOf("products").Should().Be("15 grain [GRAI].");
        }

        [Theory]
        [InlineData("orc [ORC] at $42", 1, "orc", "ORC", 42)]
        [InlineData("65 orcs [ORC] at $42", 65, "orcs", "ORC", 42)]
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
        public void Region(string input) {
            var result = AtlantisParser.Region.Parse(input);
            output.AssertParsed(result);

            var v = result.Value;
        }

        [Fact]
        public void Regions() {
            var result = AtlantisParser.Regions.Parse(@"underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town],
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
            var result = AtlantisParser.ReportLine.Parse(input);
            output.AssertParsed(result);
        }

        [Fact]
        public void MustParseTwoRegions() {
            string input = @"plain (4,4) in Prefield, contains Estenher [village], 6736 peasants
  (high elves), $5658.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $14.2 (Max: $1131).
  Wanted: 104 grain [GRAI] at $24, 109 livestock [LIVE] at $26, 131
    fish [FISH] at $20.
  For Sale: 269 high elves [HELF] at $45, 53 leaders [LEAD] at $795.
  Entertainment available: $350.
  Products: 46 livestock [LIVE], 20 horses [HORS].

Exits:
  North : ocean (4,2) in Atlantis Ocean.
  Northeast : plain (5,3) in Prefield.
  Southeast : plain (5,5) in Prefield.
  South : plain (4,6) in Prefield.
  Southwest : ocean (3,5) in Atlantis Ocean.
  Northwest : ocean (3,3) in Atlantis Ocean.

- taxmen (2333), Anuii (44), revealing faction, holding, sharing, 44
  high elves [HELF], 58 humans [MAN], 11 gnomes [GNOM], 4 swords
  [SWOR], 6268 silver [SILV]. Weight: 1079. Capacity: 0/0/1629/0.
  Skills: combat [COMB] 1 (40).
- scout (2900), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 6 high elves [HELF], horse [HORS], 275
  silver [SILV]. Weight: 110. Capacity: 0/70/160/0. Skills: riding
  [RIDI] 2 (90).
- vestnieks (927), Semigallians (18), avoiding, behind, centaur
  [CTAU].
- Ambassador Este (3397), Disasters Inc (43), avoiding, behind,
  revealing faction, human [MAN], 70 silver [SILV]. Weight: 10.
  Capacity: 0/0/15/0. Skills: none.
- p ship (3930), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 10 high elves [HELF]. Weight: 100.
  Capacity: 0/0/150/0. Skills: shipbuilding [SHIP] 5 (450).
- scout (5921), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, gnome [GNOM], 62 silver [SILV]. Weight: 5.
  Capacity: 0/0/9/0. Skills: observation [OBSE] 2 (90), riding [RIDI]
  1 (35).
- entertainers (6382), Anuii (44), avoiding, behind, revealing
  faction, weightless battle spoils, 8 gnomes [GNOM], 1680 silver
  [SILV]. Weight: 40. Capacity: 0/0/72/0. Skills: entertainment [ENTE]
  2 (125).
- workers (6782), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 5 high elves [HELF], 204 silver [SILV].
  Weight: 50. Capacity: 0/0/75/0. Skills: none.
- Emissary Prefield (2552), Ordo Hereticus (30), avoiding, behind,
  revealing faction, holding, receiving no aid, sharing, won't cross
  water, gnoll [GNOL], camel [CAME], 24 silver [SILV]. Weight: 60.
  Capacity: 0/70/85/0. Skills: none.
- scout (8038), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, human [MAN]. Weight: 10. Capacity:
  0/0/15/0. Skills: riding [RIDI] 2 (125).
- vestnieks (7480), Skalperians (22), avoiding, behind, gnome [GNOM].
- scout (3962), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, centaur [CTAU], 6 horses [HORS]. Weight:
  350. Capacity: 0/490/490/0. Skills: none.
- scout (3440), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, high elf [HELF], horse [HORS]. Weight: 60.
  Capacity: 0/70/85/0. Skills: riding [RIDI] 2 (90).
- scout (3439), Anuii (44), avoiding, behind, revealing faction,
  sharing, weightless battle spoils, high elf [HELF], horse [HORS].
  Weight: 60. Capacity: 0/70/85/0. Skills: riding [RIDI] 2 (90).
- City Guard (9478), on guard, The Guardsmen (1), 4 leaders [LEAD], 4
  swords [SWOR].

plain (6,4) in Prefield, 4787 peasants (centaurs), $4978.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $15.2 (Max: $995).
  Wanted: none.
  For Sale: 191 centaurs [CTAU] at $85, 38 leaders [LEAD] at $851.
  Entertainment available: $296.
  Products: 66 grain [GRAI], 38 horses [HORS], 6 winged horses [WING].

Exits:
  North : ocean (6,2) in Atlantis Ocean.
  Northeast : plain (7,3) in Prefield.
  Southeast : ocean (7,5) in Atlantis Ocean.
  South : plain (6,6) in Prefield.
  Southwest : plain (5,5) in Prefield.
  Northwest : plain (5,3) in Prefield.

- taxmen (839), Anuii (44), revealing faction, holding, sharing,
  weightless battle spoils, 4 centaurs [CTAU], horse [HORS]. Weight:
  250. Capacity: 0/350/350/0. Skills: combat [COMB] 1 (30).
- horses (1853), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, centaur [CTAU], 6 winged horses [WING],
  lasso [LASS]. Weight: 351. Capacity: 420/490/490/0. Skills: horse
  training [HORS] 5 (450).
- p horses (2331), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 15 humans [MAN], gnome [GNOM], 38 horses
  [HORS], 6 lassoes [LASS]. Weight: 2061. Capacity: 0/2660/2894/0.
  Skills: horse training [HORS] 2 (90).
- taxmen (2087), on guard, Anuii (44), revealing faction, holding,
  sharing, 44 gnomes [GNOM], 13 humans [MAN], 42 centaurs [CTAU], 5215
  silver [SILV]. Weight: 2450. Capacity: 0/2940/3531/0. Skills: combat
  [COMB] 1 (31).
- b grain (2673), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 5 gnomes [GNOM], 279 silver [SILV].
  Weight: 25. Capacity: 0/0/45/0. Skills: farming [FARM] 2 (97).
- centaur scout (3933), Anuii (44), avoiding, behind, revealing
  faction, weightless battle spoils, centaur [CTAU], 80 silver [SILV].
  Weight: 50. Capacity: 0/70/70/0. Skills: riding [RIDI] 3 (180).
- p horses (2088), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 14 gnomes [GNOM], 302 silver [SILV].
  Weight: 70. Capacity: 0/0/126/0. Skills: horse training [HORS] 2
  (90).
- p grain (2340), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 29 gnomes [GNOM], 4 bags [BAG], 66 grain
  [GRAI]. Weight: 479. Capacity: 0/0/261/0. Skills: farming [FARM] 2
  (124).
- entertainers (6383), Anuii (44), avoiding, behind, revealing
  faction, weightless battle spoils, 6 gnomes [GNOM], 760 silver
  [SILV]. Weight: 30. Capacity: 0/0/54/0. Skills: entertainment [ENTE]
  2 (125).
- worker (4615), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 2 humans [MAN]. Weight: 20. Capacity:
  0/0/30/0. Skills: riding [RIDI] 2 (150).
- b farmers (848), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 14 humans [MAN], 3 horses [HORS]. Weight:
  290. Capacity: 0/210/420/0. Skills: farming [FARM] 2 (137).
- b fur (3444), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, gnoll [GNOL], 11 horses [HORS], 5 silver
  [SILV]. Weight: 560. Capacity: 0/770/785/0. Skills: hunting [HUNT] 5
  (450).
- scout (8383), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, centaur [CTAU], 16 lassoes [LASS], 5
  silver [SILV]. Weight: 66. Capacity: 0/70/70/0. Skills: none.
- p fur (9300), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 3 gnolls [GNOL], 15 silver [SILV]. Weight:
  30. Capacity: 0/0/45/0. Skills: hunting [HUNT] 5 (450).
- p fur (9301), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 3 gnolls [GNOL], 15 silver [SILV]. Weight:
  30. Capacity: 0/0/45/0. Skills: hunting [HUNT] 5 (450).
- scout (8840), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, centaur [CTAU], 22 horses [HORS], 9 stone
  [STON]. Weight: 1600. Capacity: 0/1610/1610/0. Skills: none.
- centaur scout (3934), Anuii (44), avoiding, behind, revealing
  faction, weightless battle spoils, centaur [CTAU], 5 horses [HORS].
  Weight: 300. Capacity: 0/420/420/0. Skills: riding [RIDI] 3 (210).
- scout (3115), Anuii (44), avoiding, behind, revealing faction,
  sharing, weightless battle spoils, human [MAN], 10 horses [HORS].
  Weight: 510. Capacity: 0/700/715/0. Skills: none.
- p livestock (9303), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 7 ice dwarves [IDWA], 4 horses [HORS].
  Weight: 270. Capacity: 0/280/385/0. Skills: ranching [RANC] 3 (181).
- p livestock (9304), Anuii (44), avoiding, behind, revealing faction,
  weightless battle spoils, 8 ice dwarves [IDWA], 4 horses [HORS].
  Weight: 280. Capacity: 0/280/400/0. Skills: ranching [RANC] 3 (181).

+ Fleet [128] : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.
  - captain longship 128 (3941), Anuii (44), avoiding, behind,
    revealing faction, weightless battle spoils, 2 gnomes [GNOM], 83
    crossbows [XBOW], 36 herbs [HERB]. Weight: 93. Capacity: 0/0/18/0.
    Skills: sailing [SAIL] 2 (90).

+ Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors:
  10/10; MaxSpeed: 4.
  - captain fleet 121 (3940), Anuii (44), avoiding, behind, revealing
    faction, weightless battle spoils, 2 gnomes [GNOM], 11 stone
    [STON]. Weight: 560. Capacity: 0/0/18/0. Skills: sailing [SAIL] 2
    (90).
  - Unit (6795), Anuii (44), avoiding, behind, revealing faction,
    weightless battle spoils, 2 lizardmen [LIZA]. Weight: 20.
    Capacity: 0/0/30/30. Skills: sailing [SAIL] 3 (195).

";

            var result = AtlantisParser.Regions.Parse(input);
            output.AssertParsed(result);


            output.WriteLine(result.Value.ToString());
            result.Value.Children.Count.Should().Be(2);
        }

        [Theory]
        [InlineData("+ Fleet [128] : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.\n")]
        [InlineData(@"+ Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors:
  10/10; MaxSpeed: 4.
")]
        public void structure(string input) {
            var result = AtlantisParser.Structure.Parse(input);
            output.AssertParsed(result);

            output.WriteLine(result.Value.ToString());
        }

        [Theory]
        [InlineData(@"+ Fleet [128] : Longship; Load: 93/100; Sailors: 4/4; MaxSpeed: 4.
  - captain longship 128 (3941), Anuii (44), avoiding, behind,
    revealing faction, weightless battle spoils, 2 gnomes [GNOM], 83
    crossbows [XBOW], 36 herbs [HERB]. Weight: 93. Capacity: 0/0/18/0.
    Skills: sailing [SAIL] 2 (90).

")]
        [InlineData(@"+ Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors:
  10/10; MaxSpeed: 4.
  - captain fleet 121 (3940), Anuii (44), avoiding, behind, revealing
    faction, weightless battle spoils, 2 gnomes [GNOM], 11 stone
    [STON]. Weight: 560. Capacity: 0/0/18/0. Skills: sailing [SAIL] 2
    (90).
  - Unit (6795), Anuii (44), avoiding, behind, revealing faction,
    weightless battle spoils, 2 lizardmen [LIZA]. Weight: 20.
    Capacity: 0/0/30/30. Skills: sailing [SAIL] 3 (195).

")]
        public void structureWithUnits(string input) {
            var result = AtlantisParser.StructureWithUnits.Parse(input);
            output.AssertParsed(result);

            output.WriteLine(result.Value.ToString());
        }

        [Fact]
        public void canParserUnit() {
            var result = AtlantisParser.Unit<Pidgin.Unit>().Parse(@"* Unit m2 (2530), Avalon Empire (15), avoiding, behind, revealing
  faction, holding, receiving no aid, won't cross water, wood elf
  [WELF], horse [HORS]. Weight: 60. Capacity: 0/70/85/0. Skills:
  combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).
");
            output.AssertParsed(result);

            output.WriteLine(result.Value.ToString());
        }

        [Fact]
        public void canParseUnitWithDescription() {
            var result = AtlantisParser.Unit<Pidgin.Unit>().Parse(
@"- Herdsmen of the Ungre Guild (1767), 3 humans [MAN], 3 spices [SPIC],
  2 gems [GEM], net [NET], 2 mink [MINK], 3 livestock [LIVE]; Content
  looking shepherds and herdsmen.
");
            output.AssertParsed(result);

            output.WriteLine(result.Value.ToString());

        }

        [Fact]
        public void canParserUnits() {
            var result = AtlantisParser.Units<Pidgin.Unit>().Parse(@"* Unit m2 (2530), Avalon Empire (15), avoiding, behind, revealing
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

        [Theory]
        [InlineData("Weight: 60.\n*", 1)]
        public void unitAttributes(string input, int count) {
            var result = AtlantisParser.UnitAttributes.Parse(input);
            output.AssertParsed(result);

            // result.Value.Children.Count.Should().Be(count);
        }
    }
}
