namespace atlantis {
    using System.Collections.Generic;
    using System.Linq;

    public class ReportBlock {
        public ReportBlock(ReportState type, MultiLineBlock[] blocks) {
            Type = type;
            lines = blocks;
        }

        public ReportBlock(ReportState type, ReportBlock[] items) {
            Type = type;
            Items = items;
        }

        private MultiLineBlock[] lines;

        public ReportState Type { get; }
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
