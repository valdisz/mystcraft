namespace advisor
{
    // Weight: 30
    public class UnitWeightParser : BaseParser {
        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var weight = p.After("Weight:").SkipWhitespaces().Integer();
            if (!weight) return Error(weight);

            return Ok(ReportNode.Bag(
                ReportNode.Int("weight", weight)
            ));
        }
    }
}
