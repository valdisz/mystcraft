namespace advisor
{
    // Sandlings (384) attacks Mystic Masons (15876) in desert (50,28) in Feapi'iss!
    // Unit (127) attempts to assassinate Sneaky Git (8720) in mountain (49,39) in Swerthenchi!
    public class BattleHeadlineParser : BaseParser {
        readonly IReportParser unitName = new UnitNameParser();
        readonly IReportParser locationParser = new LocationParser(new CoordsParser());

        protected override Maybe<IReportNode> Execute(TextParser p) {
            p = p.BeforeBackwards("!");

            var attacker = unitName.Parse(p);
            if (!attacker) return Error(attacker);

            var defender = unitName.Parse(p.OneOf(
                _ => _.Skip(" attacks "),
                _ => _.Skip(" attempts to assassinate ")
            ).SkipWhitespaces());
            if (!defender) return Error(defender);

            var location = locationParser.Parse(p.SkipWhitespaces(minTimes: 1).After("in").SkipWhitespaces(minTimes: 1));
            if (!location) return Error(location);

            return Ok(ReportNode.Bag(
                ReportNode.Key("attacker", ReportNode.Object(attacker.Value)),
                ReportNode.Key("defender", ReportNode.Object(defender.Value)),
                ReportNode.Key("location", ReportNode.Object(location.Value))
            ));
        }
    }
}
