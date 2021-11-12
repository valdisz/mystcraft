namespace advisor
{
    // sailors (10990) is assassinated in jungle (39,35) in Su'agie!
    // Scout (83) is assassinated in mountain (49,39) in Swerthenchi!
    public class AssassinationParser : BaseParser {
        readonly IReportParser unitName = new UnitNameParser();
        readonly IReportParser locationParser = new LocationParser(new CoordsParser());

        protected override Maybe<IReportNode> Execute(TextParser p) {
            p = p.BeforeBackwards("!");

            var victim = unitName.Parse(p);
            if (!victim) return Error(victim);

            var location = locationParser.Parse(p
                .SkipWhitespaces(minTimes: 1).Skip("is")
                .SkipWhitespaces(minTimes: 1).Skip("assassinated")
                .SkipWhitespaces(minTimes: 1).Skip("in")
                .SkipWhitespaces(minTimes: 1)
            );
            if (!location) return Error(location);

            return Ok(ReportNode.Bag(
                ReportNode.Key("victim", ReportNode.Object(victim.Value)),
                ReportNode.Key("location", ReportNode.Object(location.Value))
            ));
        }
    }
}
