namespace atlantis
{
    // combat [COMB] 1 (31)
    public class SkillParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var name = p.Before("[").SkipWhitespacesBackwards().AsString();
            if (!name) return Error(name);

            var code = p.Between("[", "]").AsString();
            if (!code) return Error(code);

            p.PushBookmark();
            var level = p.SkipWhitespaces().Integer();
            var days = Maybe<int>.NA;
            if (level) {
                p.RemoveBookmark();
                days = p.Try(parser => parser.SkipWhitespaces().Between("(", ")").Integer());
            }
            else {
                p.PopBookmark();
            }

            return Ok(ReportNode.Object(
                ReportNode.Str("name", name),
                ReportNode.Str("code", code),
                level ? ReportNode.Int("level", level) : null,
                days ? ReportNode.Int("days", days) : null
            ));
        }
    }
}
