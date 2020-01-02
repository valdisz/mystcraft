namespace atlantis
{
    using System;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class FactionStatusSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Faction Status:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("factionStatus");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                var item = AllParsers.FactionStatusItem.Parse(cursor.Value);
                if (!item) {
                    throw new FormatException();
                }

                await item.Value.WriteJson(writer);
            }
            await writer.WriteEndArrayAsync();
        }
    }
}
