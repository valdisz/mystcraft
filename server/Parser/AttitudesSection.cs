namespace advisor
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class AttitudesSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Declared Attitudes");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("attitudes");
            await writer.WriteStartObjectAsync();

            var defaultAttitude = cursor.Value.After("(default").SkipWhitespaces().Word().AsString();
            await writer.WritePropertyNameAsync("default");
            await writer.WriteValueAsync(defaultAttitude.Value);


            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                var p = cursor.Value;
                var attitude = p.Word().AsString();
                var factions = p.After(":").SkipWhitespaces().BeforeBackwards(".").List(",", AllParsers.FactionName);

                await writer.WritePropertyNameAsync(attitude.Value.ToLowerInvariant());
                await writer.WriteStartArrayAsync();
                foreach (var f in factions.Value) {
                    await writer.WriteStartObjectAsync();
                    await f.WriteJson(writer);
                    await writer.WriteEndObjectAsync();
                }
                await writer.WriteEndArrayAsync();
            }

            await writer.WriteEndObjectAsync();
        }
    }
}
