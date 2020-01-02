namespace atlantis {
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

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
