namespace atlantis
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    public class AtlantisTextReader {
        public AtlantisTextReader(TextReader report) {
            this.report = report;
        }

        private readonly TextReader report;
        private readonly List<string> lines = new List<string>(32);
        private string line;
        private int ln = 0;
        private int ln0 = 0;

        private MultiLineBlock WriteBlock() {
            var block = new MultiLineBlock(ln0, lines.ToArray());
            lines.Clear();
            return block;
        }

        private void AddLine(string s = null) {
            if (lines.Count == 0) ln0 = ln;
            lines.Add(s == null ? line : s);
        }

        private async Task<string> NextLineAsync() {
            line = await report.ReadLineAsync();
            if (line != null) {
                ln++;
            }

            return line;
        }

        private async IAsyncEnumerable<MultiLineBlock> ReadStructureAsync() {
            AddLine();

            while (await NextLineAsync() != null) {
                // next line can be:

                // 1. end of structures block,
                if (string.IsNullOrWhiteSpace(line)) {
                    break;
                }

                var isIdent = line.Length >= 3 && string.IsNullOrWhiteSpace(line.Substring(0, 2));

                // 2. unit start
                if (isIdent && (line[2] == '-' || line[2] == '*')) {
                    if (lines.Count > 0) yield return WriteBlock();

                    AddLine(line.Substring(2));
                    continue;
                }

                var isUnitIdent = line.Length > 4 && string.IsNullOrWhiteSpace(line.Substring(0, 4));

                // 3. continuation of unit
                if (isUnitIdent) {
                    AddLine(line.Substring(2));
                    continue;
                }

                // 4. this is structure continuation
                AddLine();
            }

            if (lines.Count > 0) yield return WriteBlock();
        }

        public async IAsyncEnumerable<MultiLineBlock> ReadAllAsync() {
            while (await NextLineAsync() != null) {
                if (line == "") {
                    if (lines.Count > 0) yield return WriteBlock();
                    continue;
                }

                char firstChar = line[0];
                switch (firstChar) {
                    // structure have special treatment
                    case '+':
                        if (lines.Count > 0) yield return WriteBlock();
                        await foreach (var block in ReadStructureAsync()) {
                            yield return block;
                        }
                        break;

                    // if line starts with whitespace, it is added to current block
                    // if it is started
                    case var c when (char.IsWhiteSpace(c) && lines.Count > 0):
                        AddLine();
                        break;

                    // when it is not whitesapce we start new block
                    default:
                        if (lines.Count > 0) yield return WriteBlock();
                        AddLine();
                        break;
                }
            }

            // last block must be outputed
            if (lines.Count > 0) yield return WriteBlock();
        }
    }
}
