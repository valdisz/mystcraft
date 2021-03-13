namespace advisor
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class RulesetSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Atlantis Engine Version:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            var p = cursor.Value;
            var version = p.SkipWhitespaces().AsString();

            await cursor.NextAsync();
            p = cursor.Value;

            var rulesetName = p.BeforeBackwards(",").AsString();
            var rulesetVersion = p.After("Version:").SkipWhitespaces().AsString();

            await writer.WritePropertyNameAsync("engine");
            await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("version");
                await writer.WriteValueAsync(version.Value);

                await writer.WritePropertyNameAsync("ruleset");
                await writer.WriteStartObjectAsync();
                    await writer.WritePropertyNameAsync("name");
                    await writer.WriteValueAsync(rulesetName.Value);

                    await writer.WritePropertyNameAsync("version");
                    await writer.WriteValueAsync(rulesetVersion.Value);
                await writer.WriteEndObjectAsync();
            await writer.WriteEndObjectAsync();
        }
    }
}
