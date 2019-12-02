namespace atlantis
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RegionMatcher {
        public RegionMatcher(Cursor<MultiLineBlock> cursor) {
            items.Add(new ReportBlock(ReportState.RegionHeader, new[] { cursor.Value }));
            this.cursor = cursor;
        }

        private readonly List<ReportBlock> items = new List<ReportBlock>();
        private readonly Cursor<MultiLineBlock> cursor;

        public async Task<ReportBlock> MatchAsync() {
            while (await cursor.NextAsync()) {

                if (cursor.MatchOwnUnit()) {
                    items.Add(new ReportBlock(ReportState.OwnUnit, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchStructure()) {
                    items.Add(new ReportBlock(ReportState.Structure, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchRegionInfo()) {
                    items.Add(new ReportBlock(ReportState.RegionInfo, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchUnit()) {
                    items.Add(new ReportBlock(ReportState.Unit, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchRegionExits()) {
                    items.Add(new ReportBlock(ReportState.RegionExits, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchRegionGate()) {
                    items.Add(new ReportBlock(ReportState.Gate, new[] { cursor.Value }));
                    continue;
                }

                cursor.Back();
                break;
            }

            return new ReportBlock(ReportState.Region, items.ToArray());
        }
    }
}
