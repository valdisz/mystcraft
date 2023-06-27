namespace advisor
{
    public class LocationParser : BaseParser {
        public LocationParser(CoordsParser coordsParser) {
            this.coordsParser = coordsParser;
        }

        private readonly CoordsParser coordsParser;

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var terrain = p.Before("(").SkipWhitespacesBackwards().AsString();
            if (!terrain) return Error(terrain);

            var coords = coordsParser.Parse(p);
            if (!coords) return Error(coords);

            var province = p.After("in").SkipWhitespaces().OneOf(
                x => x.Before(","),
                x => x.Before("."),
                x => new PMaybe<TextParser>(x)
            ).AsString();
            if (!province) return Error(province);

            return Ok(ReportNode.Bag(
                ReportNode.Str("terrain", terrain),
                ReportNode.Key("coords", coords.Value),
                ReportNode.Str("province", province)
            ));
        }
    }
}
