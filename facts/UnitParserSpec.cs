// namespace atlantis.facts {
//     using System.Diagnostics;
//     using FluentAssertions;
//     using Pidgin;
//     using Xunit;

//     public class TextParserSpec {
//         [Fact]
//         public void UnitName() {
//             var parser = new TextParser(1, "my unit (1234)");
//             var (name, num) = parser.ParseUnitName().Value;
//         }

//         [Fact]
//         public void SkipWhitespacesBackwards() {
//             var p = new TextParser(1, "foo bar   ");
//             var result = p.SkipWhitespacesBackwards();

//             result.Value.AsString().Should().Be("foo bar");
//             p.AsString().Should().Be("   ");
//         }
//     }

//     public class UnitParsersSpec {

//         [Theory]
//         [InlineData(@"* Messengers (10991), Avalon Empire (15), avoiding, behind, holding, won't cross water, 60 centaurs [CTAU]. Weight: 3000. Capacity: 0/4200/4200/0. Skills: riding [RIDI] 3 (180), stealth [STEA] 2 (90).")]
//         public void CanParseUnit(string s) {
//             var parser = new UnitParser(new SkillParser());
//             parser.Parse(new TextParser(1, s)).Success.Should().BeTrue();
//         }
//     }
// }
