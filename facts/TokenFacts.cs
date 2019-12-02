// namespace atlantis.facts {
//     using System;
//     using Xunit;
//     using atlantis;
//     using Pidgin;
//     using Xunit.Abstractions;
//     using FluentAssertions;
//     using System.Linq;

//     public class TokenFacts {
//         public TokenFacts(ITestOutputHelper output) {
//             this.output = output;
//         }

//         private readonly ITestOutputHelper output;

//         [Theory]
//         [InlineData(@"foo", "foo")]
//         [InlineData(@"It was winter last month", "It was winter last month")]
//         [InlineData(@"foo ", "foo")]
//         [InlineData(@"foo bar", "foo bar")]
//         [InlineData(@"foo bar ", "foo bar")]
//         [InlineData(@"foo  bar", "foo  bar")]
//         [InlineData(@"foo
// bar", @"foo")]
//         [InlineData(@"foo
//   bar", @"foo bar")]
//         [InlineData(@"foo
//     bar", @"foo bar")]
//         [InlineData("Demon's Duchy", "Demon's Duchy")]
//         [InlineData("Content looking shepherds and herdsmen", "Content looking shepherds and herdsmen")]
//         [InlineData("Content looking shepherds and\n  herdsmen", "Content looking shepherds and herdsmen")]
//         [InlineData("Content looking shepherds\n  and herdsmen", "Content looking shepherds and herdsmen")]
//         [InlineData("Content looking\n  shepherds and herdsmen", "Content looking shepherds and herdsmen")]
//         [InlineData("Content\n  looking shepherds and herdsmen", "Content looking shepherds and herdsmen")]
//         public void TText_matches(string input, string expected) {
//             var res = output.AssertParsed(Tokens.TText(Tokens.AtlantisCharset).Parse(input));
//             Assert.Equal(expected, res.Value);
//         }

//         // [Theory]
//         [InlineData("Content\n  looking shepherds and herdsmen.\n", "Content looking shepherds and herdsmen")]
//         public void TText_matchesMultiLineWithTerminator(string input, string expected) {
//             var res = output
//                 .AssertParsed(Tokens.TText(Tokens.AtlantisCharset, terminator: AtlantisParser.UnitTerminator())
//                 .Parse(input));
//             Assert.Equal(expected, res.Value);
//         }
//     }
// }
