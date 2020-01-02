namespace atlantis
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class ItemReportsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Item reports:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("itemReports");
            await writer.WriteStartArrayAsync();

            Maybe<IReportNode> item = Maybe<IReportNode>.NA;
            do {
                await cursor.SkipEmptyLines();
                var p = cursor.Value;

                item = AllParsers.Item.ParseMaybe(p.Before(","));
                if (item) {
                    await writer.WriteStartObjectAsync();
                    await writer.WritePropertyNameAsync("item");
                    await item.Value.WriteJson(writer);

                    await writer.WritePropertyNameAsync("description");
                    await writer.WriteValueAsync(p.After(",").SkipWhitespaces().AsString().Value);
                    await writer.WriteEndObjectAsync();
                }
                else {
                    p.Reset();
                    cursor.Back();
                    break;
                }
            }
            while (item);

            await writer.WriteEndArrayAsync();
        }
    }
}
