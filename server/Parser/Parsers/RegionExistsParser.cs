namespace atlantis
{
    using System;
    using System.Collections.Generic;

    // Exits:
    //   North : mountain (0,2) in Davale.
    //   Northeast : mountain (1,3) in Davale.
    //   Southeast : mountain (1,5) in Davale.
    //   South : mountain (0,6) in Davale.
    //   Southwest : mountain (55,5) in Davale.
    //   Northwest : mountain (55,3) in Davale.
    // Northwest : mountain (55,3) in Davale, contains Ockwi [city]
    public class RegionExistsParser : BaseParser {
        public RegionExistsParser(LocationParser locationParser, SettlementParser settlementParser) {
            this.locationParser = locationParser;
            this.settlementParser = settlementParser;
        }

        private readonly LocationParser locationParser;
        private readonly SettlementParser settlementParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            p.SkipWhitespaces().Match("Exits:").SkipWhitespaces();

            List<IReportNode> exits = new List<IReportNode>();

            while (!p.EOF) {
                var exit = p.Before(".").SkipWhitespaces();

                p.After(".").SkipWhitespaces();

                var direction = exit.Word().AsString();
                if (!direction) return Error(direction);

                exit.After(":").SkipWhitespaces();

                var location = locationParser.Parse(exit);
                if (!location) return Error(location);

                var settlement = exit.Try(settlementParser);

                exits.Add(ReportNode.Object(
                    ReportNode.Str("direction", direction),
                    location.Value,
                    settlement ? ReportNode.Key("settlement", settlement.Value) : null
                ));
            }

            return Ok(ReportNode.Array(exits));
        }
    }
}
