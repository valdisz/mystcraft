namespace advisor {
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class BattlesSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            return Task.FromResult(cursor.Value.Match("Battles during turn:").Success);
        }

        readonly IReportParser headlineParser = new BattleHeadlineParser();
        readonly IReportParser assassinationParser = new AssassinationParser();
        readonly IReportParser unitParser = new BattleUnitParser();
        readonly IReportParser itemListParser = new ItemListParser();

        private bool CanParseAsBattle(Cursor<TextParser> cursor) {
            var result = headlineParser.Parse(cursor.Value);
            cursor.Value.Reset();

            return result.Success;
        }

        private bool CanParseAsAssassination(Cursor<TextParser> cursor) {
            var result = assassinationParser.Parse(cursor.Value);
            cursor.Value.Reset();

            return result.Success;
        }

        private async Task ParseBattleAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WriteStartObjectAsync();

            var headline = headlineParser.Parse(cursor.Value).Value;
            await headline.WriteJson(writer);

            await cursor.SkipEmptyLines();
            if (!cursor.Value.Match("Attackers:")) {
                throw new ReportParserException();
            }

            await writer.WritePropertyNameAsync("attackers");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                var unit = unitParser.Parse(cursor.Value);
                await unit.Value.WriteJson(writer);
            }
            await writer.WriteEndArrayAsync();

            await cursor.SkipEmptyLines();
            if (!cursor.Value.Match("Defenders:")) {
                throw new ReportParserException();
            }

            await writer.WritePropertyNameAsync("defenders");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                var unit = unitParser.Parse(cursor.Value);
                await unit.Value.WriteJson(writer);
            }
            await writer.WriteEndArrayAsync();

            await cursor.SkipEmptyLines();
            cursor.Back();

            StringBuilder sb = new StringBuilder();

            await writer.WritePropertyNameAsync("rounds");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.StartsWith("Total Casualties:")) {
                var p = cursor.Value;

                p.PushBookmark();
                p.Match("Round");
                p.SkipWhitespaces().Integer();
                bool isRoundStart = p.Match(":");
                p.PopBookmark();

                isRoundStart = isRoundStart || p.EndsWith(" is routed!") || p.EndsWith(" gets a free round of attacks.");
                if (isRoundStart) {
                    if (sb.Length > 0) {
                        await writer.WriteValueAsync(sb.ToString());
                        sb.Clear();
                    }
                }
                else {
                    sb.AppendLine(cursor.Value.Text.ToString());
                }
            }

            if (sb.Length > 0) {
                await writer.WriteValueAsync(sb.ToString());
                sb.Clear();
            }
            await writer.WriteEndArrayAsync();

            await writer.WritePropertyNameAsync("casualties");
            while (await cursor.NextAsync() && !cursor.Value.StartsWith("Spoils: ")) {
                sb.AppendLine(cursor.Value.Text.ToString());
            }

            await writer.WriteValueAsync(sb.ToString());
            sb.Clear();

            await writer.WritePropertyNameAsync("spoils");

            var spoils = cursor.Value.After("Spoils:").SkipWhitespaces().Before(".");
            var items = itemListParser.Parse(spoils);
            await items.Value.WriteJson(writer);

            await cursor.SkipEmptyLines();

            await writer.WriteEndObjectAsync();
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            var assassinations = new List<IReportNode>();

            await writer.WritePropertyNameAsync("battles");
            await writer.WriteStartArrayAsync();

            await cursor.NextAsync();
            while (cursor.HasValue) {
                if (CanParseAsBattle(cursor)) {
                    await ParseBattleAsync(cursor, writer);
                }
                else if (CanParseAsAssassination(cursor)) {
                    assassinations.Add(assassinationParser.Parse(cursor.Value).Value);
                }
                else {
                    break;
                }
            }

            await writer.WriteEndArrayAsync();

            await writer.WritePropertyNameAsync("assassinations");
            await writer.WriteStartArrayAsync();
            foreach (var item in assassinations) {
                await item.WriteJson(writer);
            }
            await writer.WriteEndArrayAsync();
        }
    }
}
