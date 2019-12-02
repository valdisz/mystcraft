namespace atlantis {
    using System.Linq;

    public struct MultiLineBlock {
        public MultiLineBlock(int ln, string[] text) {
            LnStart = ln;
            Lines = text;
            Text = string.Join(' ', text.Select(s => s.Trim()));
        }

        public int LnStart;
        public int LnEnd => LnStart + Lines.Length - 1;
        public string[] Lines;
        public string Text;

        public void Deconstruct(string text, out (int, int) ln, out string[] lines) {
            text = Text;
            ln = (LnStart, LnEnd);
            lines = Lines;
        }

        public override string ToString() => Text;
    }
}
