namespace atlantis {
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Text;

    public interface IReportSectionParser {
        Task<bool> CanParseAsync(Cursor<TextParser> cursor);
        Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer);
    }

    public static class AllParsers {
        public static readonly FactionNameParser FactionName = new FactionNameParser();
        public static readonly FactionArgumentParser FactionArgument = new FactionArgumentParser();
        public static readonly ReportFactionParser ReportFaction = new ReportFactionParser(FactionName, FactionArgument);
        public static readonly ReportDateParser ReportDate = new ReportDateParser();
        public static readonly FactionStatusItemParser FactionStatusItem = new FactionStatusItemParser();
        public static readonly SkillParser Skill = new SkillParser();
        public static readonly ItemParser Item = new ItemParser();
        public static readonly CoordsParser Coords = new CoordsParser();
        public static readonly RegionHeaderParser RegionHeader = new RegionHeaderParser(Coords);
        public static readonly UnitParser Unit = new UnitParser(Skill);
    }

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

    public class ErrorsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Errors during turn:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("errors");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                await writer.WriteValueAsync(cursor.Value.AsString());
            }
            await writer.WriteEndArrayAsync();
        }
    }

    public class EventsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Events during turn:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("events");
            await writer.WriteStartArrayAsync();
            while (await cursor.NextAsync() && !cursor.Value.EOF) {
                await writer.WriteValueAsync(cursor.Value.AsString());
            }
            await writer.WriteEndArrayAsync();
        }
    }

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

    public class SkillReportsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Skill reports:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("skillReports");
            await writer.WriteStartArrayAsync();

            Maybe<IReportNode> skill = Maybe<IReportNode>.NA;
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

    public class RegionsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) => IsRegion(cursor);

        private async Task<bool> IsRegion(Cursor<TextParser> cursor) {
            bool isRegion = false;
            if (await cursor.NextAsync()) {
                isRegion = cursor.Value.Match("-----");
                cursor.Value.Reset();
            }

            cursor.Back();

            return isRegion;
        }

        private bool IsUnit(TextParser p) {
            var isRegionContent = p.OneOf(
                x => x.Match("-"),
                x => x.Match("*")
            );
            p.Reset();

            return isRegionContent;
        }

        private bool IsStructure(TextParser p) {
            var isRegionContent = p.Match("+");
            p.Reset();

            return isRegionContent;
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await writer.WritePropertyNameAsync("regions");
            await writer.WriteStartArrayAsync();

            do {
                await writer.WriteStartObjectAsync();
                var h = cursor.Value.AsString();
                await AllParsers.RegionHeader.Parse(cursor.Value).Value.WriteJson(writer);

                await cursor.SkipEmptyLines();
                await writer.WritePropertyNameAsync("props");
                await writer.WriteValueAsync(cursor.Value.AsString());

                await cursor.SkipEmptyLines();
                await writer.WritePropertyNameAsync("exits");
                await writer.WriteValueAsync(cursor.Value.AsString());

                bool hasStructures = false;

                await writer.WritePropertyNameAsync("units");
                await writer.WriteStartArrayAsync();
                while (await cursor.NextAsync()) {
                    if (cursor.Value.EOF) continue;

                    if (IsUnit(cursor.Value)) {
                        var unit = AllParsers.Unit.Parse(cursor.Value);
                        if (unit) await unit.Value.WriteJson(writer);

                        continue;
                    }

                    if (IsStructure(cursor.Value)) {
                        if (!hasStructures) {
                            await writer.WriteEndArrayAsync();

                            hasStructures = true;
                            await writer.WritePropertyNameAsync("structures");
                            await writer.WriteStartArrayAsync();
                        }
                        else {
                            await writer.WriteEndArrayAsync();
                            await writer.WriteEndObjectAsync();
                        }

                        await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync("structure");
                        await writer.WriteValueAsync(cursor.Value.AsString());

                        await writer.WritePropertyNameAsync("units");
                        await writer.WriteStartArrayAsync();

                        continue;
                    }

                    cursor.Back();
                    break;
                }

                await writer.WriteEndArrayAsync();
                if (hasStructures) {
                    await writer.WriteEndObjectAsync();
                    await writer.WriteEndArrayAsync();
                }

                await writer.WriteEndObjectAsync();
            }
            while (await cursor.NextAsync() && await IsRegion(cursor));

            await writer.WriteEndArrayAsync();
            cursor.Back();
        }
    }

    public class OrdersSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Orders Template (Long Format):");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await cursor.SkipEmptyLines();
            var (number, password) = ParseOrdersHeaderLine(cursor.Value);

            await writer.WritePropertyNameAsync("ordersTemplate");
            await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("faction");
                await writer.WriteValueAsync(number);

                await writer.WritePropertyNameAsync("password");
                await writer.WriteValueAsync(password);

                await writer.WritePropertyNameAsync("units");
                await writer.WriteStartArrayAsync();

                while (await cursor.SkipUntil(c => Task.FromResult<bool>(c.Value.Match("unit")))) {
                    var p = cursor.Value;
                    var unitNumber = p.SkipWhitespaces().Integer();

                    StringBuilder orders = new StringBuilder();
                    while (await cursor.NextAsync()) {
                        if (cursor.Value.Match(";")) continue;

                        if (cursor.Value.Match("unit")) {
                            cursor.Value.Reset();
                            cursor.Back();
                            break;
                        }

                        if (cursor.Value.Match("#end")) break;

                        orders.AppendLine(cursor.Value.AsString());
                    }

                    await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync("unit");
                        await writer.WriteValueAsync(unitNumber.Value);

                        await writer.WritePropertyNameAsync("orders");
                        await writer.WriteValueAsync(orders.ToString().Trim());
                    await writer.WriteEndObjectAsync();
                }

                await writer.WriteEndArrayAsync();
            await writer.WriteEndObjectAsync();
        }

        private (int number, string password) ParseOrdersHeaderLine(TextParser p) {
            var number = p.After("#atlantis").SkipWhitespaces().Integer();
            var password = p.After("\"").BeforeBackwards("\"").AsString();

            return (number, password);
        }
    }

    public class UnclaimedSilverSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Unclaimed silver:");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            var p = cursor.Value;
            var amount = p.SkipWhitespaces().Integer();

            await writer.WritePropertyNameAsync("unclaimedSilver");
            await writer.WriteValueAsync(amount.Value);
        }
    }

    public class AtlantisReportJsonConverter : IDisposable {
        public AtlantisReportJsonConverter(TextReader reader) {
            this.reader = new AtlantisTextReader(reader);
        }

        private readonly AtlantisTextReader reader;

        private List<IReportSectionParser> sections = new List<IReportSectionParser> {
            new ReportFactionSection(),
            new RulesetSection(),
            new FactionStatusSection(),
            new ErrorsSection(),
            new EventsSection(),
            new SkillReportsSection(),
            new ItemReportsSection(),
            new AttitudesSection(),
            new UnclaimedSilverSection(),
            new RegionsSection(),
            new OrdersSection()
        };

        public async Task ConvertAsync(JsonWriter writer)
        {
            var reportLineSource = ToTextParser(reader.ReadAllAsync());
            await using var cursor = new Cursor<TextParser>(3, reportLineSource);

            await writer.WriteStartObjectAsync();
            while (await cursor.NextAsync()) {
                foreach (var section in sections) {
                    if (await section.CanParseAsync(cursor)) {
                        await section.ParseAsync(cursor, writer);
                        sections.Remove(section);
                        break;
                    }
                }
            }
            await writer.WriteEndObjectAsync();

            await writer.FlushAsync();
        }

        private static async IAsyncEnumerable<TextParser> ToTextParser(IAsyncEnumerable<MultiLineBlock> source) {
            await foreach (var block in source) {
                yield return new TextParser(block.LnStart, block.Text);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    reader.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(true);
        }
        #endregion
    }

    public static class AtlantisReportCursor {
        public static async Task<bool> SkipUntil(this Cursor<TextParser> cursor, Func<Cursor<TextParser>, Task<bool>> predicate) {
            while (await cursor.NextAsync()) {
                if (await predicate(cursor)) return true;
            }

            return false;
        }

        public static async Task<bool> SkipEmptyLines(this Cursor<TextParser> cursor) {
            while (await cursor.NextAsync()) {
                if (!cursor.Value.EOF) return true;
            }

            return false;
        }
    }
}
