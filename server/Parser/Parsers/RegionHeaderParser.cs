namespace advisor
{
    // forest (50,22) in Mapa, contains Sembury [village], 5866 peasants (high elves), $2698.
    // ocean (2,6) in Atlantis Ocean.
    public class RegionHeaderParser : BaseParser {
        public RegionHeaderParser(LocationParser locationParser, SettlementParser settlementParser) {
            this.locationParser = locationParser;
            this.settlementParser = settlementParser;
        }

        private readonly LocationParser locationParser;
        private readonly SettlementParser settlementParser;

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var location = locationParser.Parse(p);
            if (!location) return Error(location);

            var settlement = p.Try(settlementParser);

            var population = p.Try(x => {
                var amount = x.After(",").SkipWhitespaces().Integer();
                if (!amount) return amount.Convert<(int, string)>();

                var race = x.Between("(", ")").AsString();
                if (!race) return race.Convert<(int, string)>();

                return new PMaybe<(int, string)>((amount.Value, race.Value));
            });

            var tax = p.Try(x => x.After("$").Integer());

            return Ok(ReportNode.Bag(
                location.Value,
                settlement ? ReportNode.Key("settlement", settlement.Value) : null,
                population ? ReportNode.Key("population", ReportNode.Object(
                    ReportNode.Int("amount", population.Value.Item1),
                    ReportNode.Str("race", population.Value.Item2)
                )) : null,
                tax ? ReportNode.Int("tax", tax) : null
            ));
        }
    }
}
