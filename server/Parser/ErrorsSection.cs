namespace advisor
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class ErrorsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Errors during turn:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("errors");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                await writer.WriteValueAsync(cursor.Value.AsString());
            }
            await writer.WriteEndArrayAsync();
        }
    }
}
