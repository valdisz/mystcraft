namespace advisor
{
    // sailors (10990) is assassinated in jungle (39,35) in Su'agie!
    // Scout (83) is assassinated in mountain (49,39) in Swerthenchi!
    public class AssassinationParser : BaseParser {
        readonly IParser unitName = new UnitNameParser();
        readonly IParser locationParser = new LocationParser(new CoordsParser());

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var h = p.BeforeBackwards("!");
            if (!h) return Error(h);

            var victim = unitName.Parse(h);
            if (!victim) return Error(victim);

            var location = locationParser.Parse(h
                .SkipWhitespaces(minTimes: 1).Then("is")
                .SkipWhitespaces(minTimes: 1).Then("assassinated")
                .SkipWhitespaces(minTimes: 1).Then("in")
                .SkipWhitespaces(minTimes: 1)
            );
            if (!location) return Error(location);

            return Ok(ReportNode.Object(
                ReportNode.Key("victim", ReportNode.Object(victim.Value)),
                ReportNode.Key("location", ReportNode.Object(location.Value))
            ));
        }
    }
}
