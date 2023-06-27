namespace advisor
{
    // (0,0,nexus)
    // (12,34)
    // (12,34,2)
    // (12,34,2 <underworld>)
    public class CoordsParser : BaseParser {
        public const string DEFAULT_LEVEL_LABEL = "surface";
        public const int DEFAULT_LEVEL_Z = 1;

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            if (!Mem(p.Between("(", ")"))) return Error(LastResult);
            var content = LastResult.Value;

            PMaybe<int> x = content.SkipWhitespaces().Integer();
            if (!x) return Error(x);

            PMaybe<int> y = content.SkipWhitespaces().After(",").SkipWhitespaces().Integer();
            if (!y) return Error(y);

            PMaybe<int> z = new PMaybe<int>(DEFAULT_LEVEL_Z);
            PMaybe<string> label = new PMaybe<string>(DEFAULT_LEVEL_LABEL);

            if (content.SkipWhitespaces().After(",")) {
                label = content.OneOf(
                    parser => {
                        var level = parser.SkipWhitespaces().Integer();
                        if (!level) return level.Convert<string>();

                        var levelLabel = parser.SkipWhitespaces().Between("<", ">").AsString();
                        if (levelLabel) {
                           z = level;
                        }

                        return levelLabel;
                    },
                    parser => parser.SkipWhitespaces().AsString()
                );
            }

            if (label == "nexus") {
                z = new PMaybe<int>(0);
            }

            return Ok(ReportNode.Object(
                ReportNode.Int("x", x),
                ReportNode.Int("y", y),
                ReportNode.Int("z", z),
                ReportNode.Str("label", label)
            ));
        }
    }
}
