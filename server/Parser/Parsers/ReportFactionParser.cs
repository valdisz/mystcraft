namespace advisor
{
    public class ReportFactionParser : BaseParser {
        public ReportFactionParser(FactionNameParser factionName, FactionArgumentParser argumentParser) {
            this.factionName = factionName;
            this.argumentParser = argumentParser;
        }

        private readonly FactionNameParser factionName;
        private readonly FactionArgumentParser argumentParser;

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var faction = factionName.Parse(p);
            if (!faction) return Error(faction);

            var args = p.After("(").BeforeBackwards(")").List(",", argumentParser);
            if (!args) return Error(args);

            return Ok(ReportNode.Bag(
                ReportNode.Key("faction", ReportNode.Object(
                        faction.Value,
                        ReportNode.Key("type", ReportNode.Array(args.Value))
                    )
                )
            ));
        }
    }
}
