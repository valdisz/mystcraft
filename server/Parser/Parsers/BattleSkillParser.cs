using System.Linq;

namespace advisor
{
    // combat 1
    public class BattleSkillParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var w = p.Words();
            if (!w) return Error(w);

            var words = w.Value;
            if (words.Length < 2) return Error(new Maybe<IReportNode>("Must be at least 2 words", p.Ln, p.Pos));

            var l = words[words.Length - 1];
            if (!int.TryParse(l, out var level)) {
                return Error(new Maybe<IReportNode>("Not a number", p.Ln, p.Pos));
            }

            var name = string.Join(" ", words.Take(words.Length - 1));

            return Ok(ReportNode.Object(
                ReportNode.Str("name", name),
                ReportNode.Int("level", level)
            ));
        }
    }
}
