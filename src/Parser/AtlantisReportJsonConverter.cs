namespace atlantis {
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

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
            new FactionStatusSection(),
            new ErrorsSection(),
            new EventsSection(),
            new AttitudesSection(),
            new UnclaimedSilverSection()
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
                yield return new TextParser(block.Text);
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
        public static async Task SkipUntil(this Cursor<TextParser> cursor, Func<Cursor<TextParser>, Task<bool>> predicate) {
            while (await cursor.NextAsync()) {
                if (await predicate(cursor)) break;
            }
        }

        public static async Task SkipEmptyLines(this Cursor<TextParser> cursor) {
            while (await cursor.NextAsync()) {
                if (!cursor.Value.EOF) break;
            }
        }
    }
}
