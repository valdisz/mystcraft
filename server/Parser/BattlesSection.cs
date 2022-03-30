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
        readonly IReportParser healingAttempt = new HealingAttemptParser(new UnitNameParser());

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

            if (!cursor.Value.Match("Total Casualties:")) {
                throw new ReportParserException();
            }

            await ParseArmyCasualties(cursor, writer);
            await ParseArmyCasualties(cursor, writer);

            await writer.WriteEndArrayAsync();
        }

        private async Task ParseArmyCasualties(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WriteStartObjectAsync();

            await cursor.NextAsync();

            await writer.WritePropertyNameAsync("heals");
            await writer.WriteStartArrayAsync();
            Maybe<IReportNode> heal;
            while ((heal = healingAttempt.Parse(cursor.Value)) == true) {
                await heal.Value.WriteJson(writer);
                await cursor.NextAsync();
            }
            await writer.WriteEndArrayAsync();

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
                while ((p = cursor.Value.Skip(c => char.IsWhiteSpace(c) || c == ',').Integer()) == true) {
                    await writer.WriteValueAsync(p.Value);
                }
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

            var buffer = new WriteBuffer(cursor, writer);

            cursor.Back();
            int round = 0;
            while (await cursor.NextAsync() && !cursor.Value.StartsWith("Total Casualties:")) {
                var p = cursor.Value;

                if (p.Try(_ => _.Match("Battle statistics:"))) {
                    await buffer.FlushAsync();

                    await writer.WriteEndObjectAsync();
                    await writer.WriteEndArrayAsync();

                    await writer.WritePropertyNameAsync("statistics");

                    continue;
                }

                // round statistics
                if (p.Try(_ => _.Then("Round ").MatchEnd(" statistics:"))) {
                    await buffer.FlushAsync();

                    await writer.WritePropertyNameAsync("statistics");
                    continue;
                }

                // round battle log
                Maybe<TextParser> roundHeader;
                if ((roundHeader = p.Try(x => x.OneOf(
                    _ => _.Then("Round ").MatchEnd(":"),
                    _ => _.MatchEnd(" is routed!"),
                    _ => _.MatchEnd(" gets a free round of attacks.")
                ))) == true) {
                    await buffer.FlushAsync();

                    if (round++ > 0) {
                        await writer.WriteEndObjectAsync();
                    }

                    await writer.WriteStartObjectAsync();
                    await writer.WritePropertyNameAsync("log");
                    continue;
                }

                buffer.Append();
            }

            await buffer.FlushAsync();
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            var assassinations = new List<IReportNode>();

            await writer.WritePropertyNameAsync("battles");
            await writer.WriteStartArrayAsync();

            await cursor.NextAsync();
            while (cursor.HasValue) {
                if (CanParseAsBattle(cursor)) {
                    await ParseBattleAsync(cursor, writer);
                    continue;
                }

                if (CanParseAsAssassination(cursor)) {
                    assassinations.Add(assassinationParser.Parse(cursor.Value).Value);
                    await cursor.NextAsync();
                    continue;
                }

                break;
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

    // Peacekeeper (1541) heals 1 with 40% chance.
    public class HealingAttemptParser : BaseParser {
        public HealingAttemptParser(IReportParser unitName) {
            this.unitName = unitName;
        }

        private readonly IReportParser unitName;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            var unit = unitName.Parse(p);
            if (!unit) return Error(unit);

            var amount = p.After("heals").SkipWhitespaces(minTimes: 1).Integer();
            if (!amount) return Error(amount);

            var chance = p.After("with").SkipWhitespaces(minTimes: 1).Integer();
            if (!chance) return Error(chance);

            return Ok(ReportNode.Object(
                unit.Value,
                ReportNode.Int("amount", amount),
                ReportNode.Int("chance", chance)
            ));
        }
    }
}
