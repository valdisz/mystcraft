// namespace facts
// {
//     using System;
//     using Xunit;
//     using atlantis;
//     using Pidgin;
//     using Xunit.Abstractions;
//     using System.Linq;

//     public class TokenFacts {
//         public TokenFacts(ITestOutputHelper output) {
//             this.output = output;
//         }

//         private readonly ITestOutputHelper output;
//         [Fact]
//         public void TWord_matches_just_one_word() {
//             var res = output.PrintErrors(Tokens.TWord.Parse("foo bar"));

//             Assert.Equal("foo", res.Value);
//         }

//         [Fact]
//         public void Space_is_not_letter() {
//             Assert.False(char.IsLetter(' '));
//         }

//         [Theory]
//         [InlineData(@"foo", "foo")]
//         [InlineData(@"It was winter last month", "It was winter last month")]
//         [InlineData(@"foo ", "foo")]
//         [InlineData(@"foo bar", "foo bar")]
//         [InlineData(@"foo bar ", "foo bar")]
//         [InlineData(@"foo  bar", "foo  bar")]
//         [InlineData(@"foo
// bar", @"foo
// bar")]
//         [InlineData(@"foo
//  bar", @"foo
//  bar")]
//         public void TText_matches(string input, string expected) {
//             var res = output.PrintErrors(Tokens.TSentence.Parse(input));

//             Assert.Equal(expected, res.Value);
//         }

//         [Fact]
//         public void TText_must_not_match() {
//             var res = output.PrintErrors(Tokens.TSentence.Parse("foo ["));

//             Assert.Equal("foo", res.Value);
//         }
//     }

//     // swamp (1,13) in Sines, contains Grandola [town]
// }
