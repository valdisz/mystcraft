namespace advisor.facts
{
    using advisor.Features;
    using FluentAssertions;
    using Xunit;

    public class CreateGameHandlerSpec {
        [Fact]
        public void FormatVersion() {
            var handler = new CreateLocalGameHandler(null);

            handler.FormatVersion(328196).Should().Be("5.2.4");
            handler.FormatVersion(196608).Should().Be("3.0.0");
        }
    }
}
