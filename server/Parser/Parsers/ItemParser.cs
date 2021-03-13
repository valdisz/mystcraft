namespace advisor
{
    // high elf [HELF]
    // 2 high elves [HELF]
    // unlimited high elves [HELF]
    // high elf [HELF] at $34
    // 2 high elves [HELF] at $48
    // unlimited high elves [HELF] at $75
    public class ItemParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            p.PushBookmark();
            var word = p.Word();
            if (!word) return Error(word);

            int amount = 1;
            if (word.Match("unlimited")) {
                amount = -1;
                p.RemoveBookmark();
            }
            else {
                var intAmount = word.Integer();
                if (intAmount) {
                    amount = intAmount.Value;
                    p.RemoveBookmark();
                }
                else {
                    p.PopBookmark();
                }
            }

            Maybe<string> name = p.SkipWhitespaces().Before("[").SkipWhitespacesBackwards().AsString();
            if (!name) return Error(name);

            Maybe<string> code = p.Between("[", "]").AsString();
            if (!code) return Error(code);

            Maybe<int> price = p.Try(parser => parser.SkipWhitespaces().Word().Match("at")
                ? p.SkipWhitespaces().Word().Seek(1).Integer()
                : Maybe<int>.NA
            );

            return Ok(ReportNode.Object(
                ReportNode.Int("amount", amount),
                ReportNode.Str("name", name),
                ReportNode.Str("code", code),
                price ? ReportNode.Int("price", price) : null
            ));
        }
    }
}
