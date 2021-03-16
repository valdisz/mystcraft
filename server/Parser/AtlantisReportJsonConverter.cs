namespace advisor {
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class AtlantisReportJsonConverter : IDisposable {
        public AtlantisReportJsonConverter(TextReader reader)
        {
            this.reader = new AtlantisTextReader(reader);
            this.sections = new IReportSectionParser[] {
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
        }

        public AtlantisReportJsonConverter(TextReader reader, params IReportSectionParser[] sections)
        {
            this.reader = new AtlantisTextReader(reader);
            this.sections = sections;
        }

        public AtlantisReportJsonConverter(TextReader reader, IEnumerable<IReportSectionParser> sections) {
            this.reader = new AtlantisTextReader(reader);
            this.sections = sections.ToArray();
        }

        private readonly AtlantisTextReader reader;
        private readonly IReportSectionParser[] sections;

        public async Task ReadAsJsonAsync(JsonWriter writer)
        {
            var s = sections.ToList();

            await using var cursor = reader.AsCursor();

            await writer.WriteStartObjectAsync();
            while ( s.Count > 0 && await cursor.NextAsync()) {
                foreach (var section in s) {
                    if (await section.CanParseAsync(cursor)) {
                        await section.ParseAsync(cursor, writer);
                        s.Remove(section);
                        break;
                    }

                    cursor.Value.Reset();
                }
            }
            await writer.WriteEndObjectAsync();
            await writer.FlushAsync();
        }

        public async Task<JObject> ReadAsJsonAsync()
        {
            using var buffer = new MemoryStream();
            using var bufferWriter = new StreamWriter(buffer);
            using JsonWriter writer = new JsonTextWriter(bufferWriter);

            await ReadAsJsonAsync(writer);

            buffer.Seek(0, SeekOrigin.Begin);

            using var bufferReader = new StreamReader(buffer);
            using JsonReader reader = new JsonTextReader(bufferReader);

            JObject report = await JObject.LoadAsync(reader);
            return report;
        }

        public async Task<T> ReadAs<T>()
        {
            using var buffer = new MemoryStream();
            using var bufferWriter = new StreamWriter(buffer);
            using JsonWriter writer = new JsonTextWriter(bufferWriter);

            await ReadAsJsonAsync(writer);

            buffer.Seek(0, SeekOrigin.Begin);

            using var bufferReader = new StreamReader(buffer);
            using JsonReader reader = new JsonTextReader(bufferReader);

            var serializer = JsonSerializer.Create();
            var value = serializer.Deserialize<T>(reader);

            return value;
        }

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
    }
}
