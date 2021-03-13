namespace advisor
{
    // War 1
    // Trade 1
    // Magic 1
    public class FactionArgumentParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var key = p.BeforeBackwards(" ").SkipWhitespaces().AsString();
            if (!key) return Error(key);

            var value = p.SkipWhitespaces().Integer();
            if (!value) return Error(value);

            return Ok(ReportNode.Object(
                ReportNode.Str("key", key),
                ReportNode.Int("value", value)
            ));
        }
    }
}
