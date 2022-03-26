namespace advisor {
    public class FactionStatusItemParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var key = p.Before(":").AsString();
            if (!key) return Error(key);

            var amount = p.After(":").SkipWhitespaces().Integer();
            if (!amount) return Error(amount);

            var max = p.After("(").Integer();
            if (!max) return Error(max);

            return Ok(ReportNode.Object(
                ReportNode.Str("key", key),
                ReportNode.Int("amount", amount),
                ReportNode.Int("max", max)
            ));
        }
    }
}
