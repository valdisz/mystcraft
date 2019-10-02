// namespace facts
// {
//     using Xunit;
//     using atlantis;
//     using Pidgin;
//     using Xunit.Abstractions;

//     public class AtlantisParserFacts {
//         public AtlantisParserFacts(ITestOutputHelper output) {
//             this.output = output;
//         }

//         private readonly ITestOutputHelper output;

//         [Theory]
//         [InlineData("(10,10)", 10, 10, null)]
//         [InlineData("(3,8,underworld)", 3, 8, "underworld")]
//         public void CanParseLocation(string input, int x, int y, string level) {
//             var res = output.PrintErrors(AtlantisParser.Coords.Parse(input));

//             Assert.Equal(x, res.Value.X);
//             Assert.Equal(y, res.Value.Y);
//             Assert.Equal(level, res.Value.Level);
//         }

//         [Theory]
//         [InlineData("contains Berneray [city]", "Berneray")]
//         [InlineData("contains Grandola [town]", "Grandola")]
//         [InlineData("contains Morilzarak [village]", "Morilzarak")]
//         [InlineData(@"contains Morilzarak
//     [village]", "Morilzarak")]
//         [InlineData(@"contains
//     Morilzarak [village]", "Morilzarak")]
//         public void CanParseSettlement(string input, string name) {
//             var res = output.PrintErrors(AtlantisParser.Settlement.Parse(input));

//             Assert.Equal(name, res.Value.Name);
//         }

//         [Theory]
//         [InlineData("swamp (1,13) in Sines, contains Grandola [town]", "swamp", 1, 13, null, "Sines", "Grandola")]
//         public void CanParseRegionBase(string input, string terrain, int x, int y, string level, string province, string name) {
//             var res = output.PrintErrors(AtlantisParser.RegionInfo.Parse(input));

//             Assert.Equal(terrain, res.Value.Terrain);
//             Assert.Equal(x, res.Value.Coords.X);
//             Assert.Equal(y, res.Value.Coords.Y);
//             Assert.Equal(level, res.Value.Coords.Level);
//             Assert.Equal(province, res.Value.Province);
//             Assert.Equal(name, res.Value.Settlement.Name);
//         }

//         [Theory]
//         [InlineData(@"5459 peasants (sea elves)")]
//         [InlineData(@"5459 peasants
//     (sea elves)")]
//         [InlineData(@"5459
//     peasants (sea elves)")]
//         [InlineData(@"5459 peasants (orcs)")]
//         [InlineData(@"5459 peasants
//     (orcs)")]
//         [InlineData(@"5459
//     peasants (orcs)")]
//         public void CarParseRegionPopulation(string input) {
//             var res = output.PrintErrors(AtlantisParser.Population.Parse(input));
//         }

//         [Theory]
//         [InlineData(@"livestock [LIVE]")]
//         [InlineData(@"livestock
//     [LIVE]")]
//         [InlineData(@"44 livestock [LIVE]")]
//         [InlineData(@"44 livestock
//     [LIVE]")]
//         [InlineData(@"44
//     livestock [LIVE]")]
//         [InlineData(@"51 chain armor [CARM]")]
//         [InlineData(@"51 chain armor
//     [CARM]")]
//         [InlineData(@"51 chain
//     armor [CARM]")]
//         [InlineData(@"51
//     chain armor [CARM]")]
//         [InlineData(@"unlimited swords [SWOR]")]
//         [InlineData(@"unlimited swords
//     [SWOR]")]
//         [InlineData(@"unlimited
//     swords [SWOR]")]
//         public void CanParseItem(string input) {
//             var res = output.PrintErrors(AtlantisParser.Item.Parse(input));
//         }

//         [Theory]
//         [InlineData("unlimited")]
//         [InlineData("100")]
//         public void CanParseAmount(string input) {
//             output.PrintErrors(AtlantisParser.Amount.Parse(input));
//         }

//         [Theory]
//         [InlineData("$356")]
//         public void CanParseSilver(string input) {
//             output.PrintErrors(AtlantisParser.Silver.Parse(input));
//         }

//         [Theory]
//         [InlineData(@"livestock [LIVE] at $79")]
//         [InlineData(@"livestock [LIVE] at
//     $79")]
//         [InlineData(@"livestock [LIVE]
//     at $79")]
//         [InlineData(@"livestock
//     [LIVE] at $79")]
//         [InlineData(@"44 livestock [LIVE] at $79")]
//         [InlineData(@"44 livestock [LIVE] at
//     $79")]
//         [InlineData(@"44 livestock [LIVE]
//     at $79")]
//         [InlineData(@"44 livestock
//     [LIVE] at $79")]
//         [InlineData(@"44
//     livestock [LIVE] at $79")]
//         [InlineData(@"51 chain armor [CARM] at $154")]
//         [InlineData(@"51 chain armor [CARM] at
//     $154")]
//         [InlineData(@"51 chain armor [CARM]
//     at $154")]
//         [InlineData(@"51 chain armor
//     [CARM] at $154")]
//         [InlineData(@"51 chain
//     armor [CARM] at $154")]
//         [InlineData(@"51
//     chain armor [CARM] at $154")]
//         [InlineData(@"unlimited nomads [NOMA] at $72")]
//         [InlineData(@"unlimited nomads [NOMA] at
//     $72")]
//         [InlineData(@"unlimited nomads [NOMA]
//     at $72")]
//         [InlineData(@"unlimited nomads
//     [NOMA] at $72")]
//         [InlineData(@"unlimited
//     nomads [NOMA] at $72")]
//         public void CanParseItemWithPrice(string input) {
//             var res = output.PrintErrors(AtlantisParser.ItemWithPrice.Parse(input));
//         }

//         [Theory]
//         [InlineData(@"plain (49,17) in Inthon, contains Plondmark [city], 19564 peasants
//   (humans), $28172.
// ------------------------------------------------------------
//   Wages: $17.2 (Max: $5634).
//   Wanted: 126 grain [GRAI] at $24, 148 livestock [LIVE] at $18, 134
//     fish [FISH] at $26, 35 hammers [HAMM] at $95, 33 leather armor
//     [LARM] at $90, 32 chain armor [CARM] at $99, 36 picks [PICK] at
//     $120, 63 spices [SPIC] at $300, 56 mink [MINK] at $288.
//   For Sale: 64 vodka [VODK] at $97, 38 perfume [PERF] at $94, 782
//     humans [MAN] at $55, 156 leaders [LEAD] at $963.
//   Entertainment available: $1604.
//   Products: 61 livestock [LIVE], 26 horses [HORS].

// Exits:
//   North : ocean (49,15) in Atlantis Ocean.
//   Northeast : plain (50,16) in Inthon.
//   Southeast : plain (50,18) in Inthon.
//   South : plain (49,19) in Inthon.
//   Southwest : forest (48,18) in Mapa.
//   Northwest : forest (48,16) in Mapa.
// ")]
//         public void CarParseRegion(string input) {
//             var res = output.PrintErrors(AtlantisParser.Region.Parse(input));

//             var region = res.Value;

//             Assert.Equal("plain", region.Info.Terrain);

//             // (terrain, res.Value.Terrain);
//             // Assert.Equal(x, res.Value.Coords.X);
//             // Assert.Equal(y, res.Value.Coords.Y);
//             // Assert.Equal(level, res.Value.Coords.Level);
//             // Assert.Equal(province, res.Value.Province);
//             // Assert.Equal(name, res.Value.Settlement?.Name);
//             // Assert.Equal(type, res.Value.Settlement?.Type);
//         }
//     }
// }
