namespace advisor
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class SkillReportsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Skill reports:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("skillReports");
            await writer.WriteStartArrayAsync();

            PMaybe<IReportNode> skill = PMaybe<IReportNode>.NA;
            do {
                await cursor.SkipEmptyLines();
                var p = cursor.Value;

                skill = AllParsers.Skill.ParseMaybe(p.Before(":"));
                if (skill) {
                    await writer.WriteStartObjectAsync();
                    await writer.WritePropertyNameAsync("skill");
                    await skill.Value.WriteJson(writer);

                    await writer.WritePropertyNameAsync("description");
                    await writer.WriteValueAsync(p.After(":").SkipWhitespaces().AsString().Value);
                    await writer.WriteEndObjectAsync();
                }
                else {
                    p.Reset();
                    cursor.Back();
                    break;
                }
            }
            while (skill);

            await writer.WriteEndArrayAsync();
        }
    }
}
