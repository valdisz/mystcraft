namespace advisor
{
    public class SettlementParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var name = p.After("contains").SkipWhitespaces().Before("[").SkipWhitespacesBackwards().AsString();
            if (!name) return Error(name);

            var size = p.Between("[", "]").AsString();
            if (!size) return Error(size);

            return Ok(ReportNode.Object(
                ReportNode.Str("name", name),
                ReportNode.Str("size", size)
            ));
        }
    }
}
