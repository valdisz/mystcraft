namespace advisor
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class EventsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Events during turn:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("events");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                await AllParsers.Event.Parse(cursor.Value).Value.WriteJson(writer);
            }
            await writer.WriteEndArrayAsync();
        }
    }
}
