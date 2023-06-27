namespace advisor
{
    public class UnitNameParser : BaseParser {
        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var name = p.Before("(").SkipWhitespacesBackwards().AsString();
            if (!name) return Error(name);

            var num = p.Between("(", ")").Integer();
            if (!num) return Error(num);

            return Ok(ReportNode.Bag(
                ReportNode.Str("name", name),
                ReportNode.Int("number", num)
            ));
        }
    }
}
