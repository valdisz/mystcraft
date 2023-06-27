namespace advisor {
    public class RegionGateParser : BaseParser {
        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var gate = p
                .After("There is a Gate here")
                .SkipWhitespaces()
                .Between("(", ")")
                .After("Gate")
                .SkipWhitespaces()
                .Word()
                .Integer();
            if (!gate) return Error(gate);

            return Ok(ReportNode.Bag(
                ReportInt.Int("gate", gate)
            ));
        }
    }
}
