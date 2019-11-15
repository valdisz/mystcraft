namespace atlantis {
    using System.Collections.Generic;
    using System.Linq;

    public class ReportBlock {
        public ReportBlock(ReportBlockType type, MultiLineBlock[] blocks) {
            Type = type;
            lines = blocks;
        }

        public ReportBlock(ReportBlockType type, ReportBlock[] items) {
            Type = type;
            Items = items;
        }

        private MultiLineBlock[] lines;

        public ReportBlockType Type { get; }
        public ReportBlock[] Items { get; }

        public IEnumerable<MultiLineBlock> Lines {
            get {
                if (Items != null) {
                    return Items.SelectMany(x => x.Lines);
                }

                return lines;
            }
        }
    }
}
