namespace advisor
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class ReportFactionSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Atlantis Report For:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await cursor.NextAsync();
            await AllParsers.ReportFaction.Parse(cursor.Value).Value.WriteJson(writer);

            await cursor.NextAsync();
            await AllParsers.ReportDate.Parse(cursor.Value).Value.WriteJson(writer);
        }
    }
}
