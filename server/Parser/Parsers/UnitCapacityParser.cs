namespace advisor
{
    // Capacity: 0/0/45/0
    public class UnitCapacityParser : BaseParser {
        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var capacities = p.After("Capacity:").SkipWhitespaces();
            if (!capacities) return Error(capacities);

            var flying = capacities.Integer();
            if (!flying) return Error(flying);

            var riding = capacities.After("/").Integer();
            if (!riding) return Error(riding);

            var walking = capacities.After("/").Integer();
            if (!walking) return Error(walking);

            var swimming = capacities.After("/").Integer();
            if (!swimming) return Error(swimming);

            return Ok(ReportNode.Bag(
                ReportNode.Key("capacity", ReportNode.Object(
                    ReportNode.Int("flying", flying),
                    ReportNode.Int("riding", riding),
                    ReportNode.Int("walking", walking),
                    ReportNode.Int("swimming", swimming)
                ))
            ));
        }
    }
}
