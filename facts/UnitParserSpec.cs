namespace atlantis.facts {
    using System.Diagnostics;
    using FluentAssertions;
    using Pidgin;
    using Xunit;

    public class TextParserSpec {
        [Fact]
        public void UnitName() {
            var parser = new TextParser("my unit (1234)");
            var (name, num) = parser.ParseUnitName().Value;
        }

        [Fact]
        public void SkipWhitespacesBackwards() {
            var p = new TextParser("foo bar   ");
            var result = p.SkipWhitespacesBackwards();

            result.Value.AsString().Should().Be("foo bar");
            p.AsString().Should().Be("   ");
        }
    }
}
