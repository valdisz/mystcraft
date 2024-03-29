namespace advisor {
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class RegionsSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) => IsRegion(cursor);

        private async Task<bool> IsRegion(Cursor<TextParser> cursor) {
            bool isRegionHeader() {
                var p = cursor.Value.Pos;
                cursor.Value.SkipChar('-');
                var len = cursor.Value.Pos - p;

                cursor.Value.Reset();

                return len > 10;
            }

            if (isRegionHeader()) return true;

            if (!await cursor.NextAsync()) return false;

            var isReg = isRegionHeader();
            cursor.Back();

            return isReg;
        }

        private bool IsUnit(TextParser p) {
            bool isRegionContent = p.OneOf("- ", "* ", "! ", "% ", ": ", "= ");
            p.Reset();

            return isRegionContent;
        }

        private bool IsStructure(TextParser p) {
            var isRegionContent = p.Match("+ ");
            p.Reset();

            return isRegionContent;
        }

        private bool IsGate(TextParser p) {
            var isRegionContent = p.Match("There is a Gate here");
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

                var regions = AllParsers.RegionProps.Parse(cursor.Value);
                if (!regions) {
                    throw new ReportParserException(regions.Error);
                }

                await regions.Value.WriteJson(writer);

                await cursor.SkipEmptyLines();
                await writer.WritePropertyNameAsync("exits");

                var exits = AllParsers.RegionExits.Parse(cursor.Value);
                if (!exits) {
                    throw new ReportParserException(exits.Error);
                }
                await exits.Value.WriteJson(writer);

                while (await cursor.NextAsync()) {
                    if (cursor.Value.EOF) continue;

                    if (IsUnit(cursor.Value) || IsStructure(cursor.Value)) {
                        cursor.Back();
                        break;
                    }

                    if (IsGate(cursor.Value)) {
                        await AllParsers.RegionGate.Parse(cursor.Value).Value.WriteJson(writer);
                        break;
                    }

                    cursor.Back();
                    break;
                }

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

                        var structure = AllParsers.Structure.Parse(cursor.Value);
                        if (structure) await structure.Value.WriteJson(writer);

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
            while (await cursor.SkipEmptyLines() && await IsRegion(cursor));

            await writer.WriteEndArrayAsync();
            cursor.Back();
        }
    }
}
