namespace advisor
{
    // Sandlings (384) attacks Mystic Masons (15876) in desert (50,28) in Feapi'iss!
    // Unit (127) attempts to assassinate Sneaky Git (8720) in mountain (49,39) in Swerthenchi!
    public class BattleHeadlineParser : BaseParser {
        readonly IReportParser unitName = new UnitNameParser();
        readonly IReportParser locationParser = new LocationParser(new CoordsParser());

        protected override Maybe<IReportNode> Execute(TextParser p) {
            var h = p.BeforeBackwards("!");
            if (!h) return Error(h);

            var attacker = unitName.Parse(h);
            if (!attacker) return Error(attacker);

            Mem(h.OneOf(
                _ => _.Then(" attacks "),
                _ => _.Then(" attempts to assassinate ")
            ));
            if (!LastResult) return Error(LastResult);

            var defender = unitName.Parse(h.SkipWhitespaces());
            if (!defender) return Error(defender);

            var location = locationParser.Parse(h.SkipWhitespaces(minTimes: 1).After("in").SkipWhitespaces(minTimes: 1));
            if (!location) return Error(location);

            return Ok(ReportNode.Bag(
                ReportNode.Key("attacker", ReportNode.Object(attacker.Value)),
                ReportNode.Key("defender", ReportNode.Object(defender.Value)),
                ReportNode.Key("location", ReportNode.Object(location.Value))
            ));
        }
    }
}
