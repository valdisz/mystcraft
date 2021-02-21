namespace atlantis
{
    // Weight: 30
    public class UnitWeightParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var weight = p.After("Weight:").SkipWhitespaces().Integer();
            if (!weight) return Error(weight);

            return Ok(ReportNode.Bag(
                ReportNode.Int("weight", weight)
            ));
        }
    }
}
