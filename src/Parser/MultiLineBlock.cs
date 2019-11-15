namespace atlantis
{
    using System.Linq;

    public struct MultiLineBlock {
        public MultiLineBlock(int ln, string[] text) {
            Ln = ln;
            Lines = text;
            Text = string.Join(' ', text.Select(s => s.Trim()));
        }

        public int Ln;
        public string[] Lines;
        public string Text;

        public void Deconstruct(out int ln, out string text, out string[] lines) {
            ln = Ln;
            text = Text;
            lines = Lines;
        }

        public override string ToString() => Text;
    }
}
