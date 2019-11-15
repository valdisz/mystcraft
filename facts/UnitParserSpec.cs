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
    }
}
