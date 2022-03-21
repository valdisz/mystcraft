namespace advisor {
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class BattlesSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            return Task.FromResult(cursor.Value.Match("Battles during turn:").Success);
        }

        class WriteBuffer {
            public WriteBuffer(Cursor<TextParser> cursor, JsonWriter writer) {
                this.cursor = cursor;
                this.writer = writer;
            }

            private readonly Cursor<TextParser> cursor;
            private readonly JsonWriter writer;
            private readonly StringBuilder sb = new StringBuilder();

            public void Append() {
                sb.AppendLine(cursor.Value.Text.ToString());
            }

            public async Task FlushAsync() {
                if (sb.Length == 0) {
                    return;
                }

                var s = sb.ToString().Trim().Trim('\n', '\r');
                await writer.WriteValueAsync(s);
                sb.Clear();
            }
        }

        readonly IReportParser headlineParser = new BattleHeadlineParser();
        readonly IReportParser assassinationParser = new AssassinationParser();
        readonly IReportParser unit = new BattleUnitParser();
        readonly IReportParser unitName = new UnitNameParser();
        readonly IReportParser itemList = new ItemListParser();
        readonly IReportParser item = new ItemParser();

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

        private async Task ParseBattleAsync(Cursor<TextParser> cursor, JsonWriter writer)
        {
            await writer.WriteStartObjectAsync();

            await ParseHeader(cursor, writer);
            await ParseParticipants("Attackers:", "attackers", cursor, writer);
            await ParseParticipants("Defenders:", "defenders", cursor, writer);
            await ParseBattleLog(cursor, writer);
            await ParseCasualties(cursor, writer);
            await ParseSpoils(cursor, writer);
            await ParseSoldiersRose(cursor, writer);

            await writer.WriteEndObjectAsync();
        }

        private async Task ParseParticipants(string keyword, string property, Cursor<TextParser> cursor, JsonWriter writer) {
            await cursor.SkipEmptyLines();
            if (!cursor.Value.Match(keyword)) {
                throw new ReportParserException();
            }

            await writer.WritePropertyNameAsync(property);
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                var unit = this.unit.Parse(cursor.Value);
                await unit.Value.WriteJson(writer);
            }
            await writer.WriteEndArrayAsync();
        }

        private async Task ParseHeader(Cursor<TextParser> cursor, JsonWriter writer)
        {
            var headline = headlineParser.Parse(cursor.Value).Value;
            await headline.WriteJson(writer);
        }

        private async Task ParseSoldiersRose(Cursor<TextParser> cursor, JsonWriter writer)
        {
            await cursor.SkipEmptyLines();

            // soldiers rising
            Maybe<IReportNode> unitItem;
            List<IReportNode> rose = new();
            while ((unitItem = item.Parse(cursor.Value)) == true)
            {
                rose.Add(unitItem.Value);
                await cursor.SkipEmptyLines();
            }

            cursor.Value.Reset();

            await writer.WritePropertyNameAsync("rose");
            await writer.WriteStartArrayAsync();
            foreach (var item in rose)
            {
                await item.WriteJson(writer);
            }
            await writer.WriteEndArrayAsync();
        }

        private async Task ParseSpoils(Cursor<TextParser> cursor, JsonWriter writer) {
            await cursor.SkipEmptyLines();

            await writer.WritePropertyNameAsync("spoils");

            var spoils = cursor.Value.After("Spoils:").SkipWhitespaces().Before(".");
            var items = itemList.Parse(spoils);
            await items.Value.WriteJson(writer);
        }

        private async Task ParseCasualties(Cursor<TextParser> cursor, JsonWriter writer)
        {
            await writer.WritePropertyNameAsync("casualties");
            await writer.WriteStartArrayAsync();

            if (!cursor.Value.Match("Total Casualties:"))
            {
                throw new ReportParserException();
            }

            await ParseArmyCasualties(cursor, writer);
            await ParseArmyCasualties(cursor, writer);

            await writer.WriteEndArrayAsync();
        }

        private async Task ParseArmyCasualties(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WriteStartObjectAsync();

            await cursor.NextAsync();

            await writer.WritePropertyNameAsync("army");
            var army = unitName.Parse(cursor.Value);

            await writer.WriteStartObjectAsync();
            await army.Value.WriteJson(writer);
            await writer.WriteEndObjectAsync();

            await writer.WritePropertyNameAsync("lost");
            var lost = cursor.Value.SkipWhitespaces(minTimes: 1).Then("loses").SkipWhitespaces(minTimes: 1).Integer();
            await writer.WriteValueAsync(lost.Value);

            await cursor.NextAsync();

            await writer.WritePropertyNameAsync("damagedUnits");
            await writer.WriteStartArrayAsync();
            if (cursor.Value.Try(_ => _.Then("Damaged units:"))) {
                Maybe<int> p;
                do {
                    p = cursor.Value.SkipWhitespaces().Integer();
                    if (p) {
                        await writer.WriteValueAsync(p.Value);
                    }
                }
                while (p);
            }
            else {
                cursor.Back();
            }
            await writer.WriteEndArrayAsync();

            await writer.WriteEndObjectAsync();
        }

        private static async Task ParseBattleLog(Cursor<TextParser> cursor, JsonWriter writer) {
            await cursor.SkipEmptyLines();

            await writer.WritePropertyNameAsync("rounds");
            await writer.WriteStartArrayAsync();
            bool battleStats = false;

            var buffer = new WriteBuffer(cursor, writer);

            cursor.Back();
            while (await cursor.NextAsync() && !cursor.Value.StartsWith("Total Casualties:")) {
                var p = cursor.Value;

                if (p.Try(_ => _.Match("Battle statistics:"))) {
                    await buffer.FlushAsync();
                    battleStats = true;
                    continue;
                }

                // round statistics
                if (p.Try(_ => _.Then("Round ").MatchEnd(" statistics:"))) {
                    await buffer.FlushAsync();

                    await writer.WritePropertyNameAsync("statistics");
                    continue;
                }

                // round battle log
                if (p.Try(x => x.OneOf(
                    _ => _.Then("Round ").MatchEnd(":"),
                    _ => _.MatchEnd(" is routed!"),
                    _ => _.MatchEnd(" gets a free round of attacks.")
                ))) {
                    await buffer.FlushAsync();

                    if (writer.WriteState == WriteState.Object) {
                        await writer.WriteEndObjectAsync();
                    }

                    await writer.WriteStartObjectAsync();
                    await writer.WritePropertyNameAsync("log");
                    continue;
                }

                buffer.Append();
            }

            if (!battleStats) {
                await buffer.FlushAsync();
            }

            if (writer.WriteState == WriteState.Object) {
                await writer.WriteEndObjectAsync();
            }
            await writer.WriteEndArrayAsync();

            if (battleStats) {
                await writer.WritePropertyNameAsync("statistics");
                await buffer.FlushAsync();
            }
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
