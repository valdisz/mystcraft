namespace atlantis
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class RegionMatcher {
        public RegionMatcher(Cursor<MultiLineBlock> cursor) {
            items.Add(new ReportBlock(ReportBlockType.RegionHeader, new[] { cursor.Value }));
            this.cursor = cursor;
        }

        private readonly List<ReportBlock> items = new List<ReportBlock>();
        private readonly Cursor<MultiLineBlock> cursor;

        public async Task<ReportBlock> MatchAsync() {
            while (await cursor.NextAsync()) {

                if (cursor.MatchOwnUnit()) {
                    items.Add(new ReportBlock(ReportBlockType.OwnUnit, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchStructure()) {
                    items.Add(new ReportBlock(ReportBlockType.Structure, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchRegionInfo()) {
                    items.Add(new ReportBlock(ReportBlockType.RegionInfo, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchUnit()) {
                    items.Add(new ReportBlock(ReportBlockType.Unit, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchRegionExits()) {
                    items.Add(new ReportBlock(ReportBlockType.RegionExits, new[] { cursor.Value }));
                    continue;
                }

                if (cursor.MatchRegionGate()) {
                    items.Add(new ReportBlock(ReportBlockType.Gate, new[] { cursor.Value }));
                    continue;
                }

                cursor.Back();
                break;
            }

            return new ReportBlock(ReportBlockType.Region, items.ToArray());
        }
    }
}
