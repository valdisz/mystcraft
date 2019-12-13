namespace atlantis
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class UnclaimedSilverSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Unclaimed silver:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            var p = cursor.Value;
            var amount = p.SkipWhitespaces().Integer();

            await writer.WritePropertyNameAsync("unclaimedSilver");
            await writer.WriteValueAsync(amount.Value);
        }
    }
}
