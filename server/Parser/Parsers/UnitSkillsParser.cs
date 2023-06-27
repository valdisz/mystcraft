using System.Collections.Generic;

namespace advisor
{
    // Skills: endurance [ENDU] 1 (30)
    // Skills: fire [FIRE] 1 (30), earthquake [EQUA] 1 (30)
    public class UnitSkillsParser : BaseParser {
        public UnitSkillsParser(IParser skillParser) {
            this.skillParser = skillParser;
        }

        private readonly IParser skillParser;

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var prefix = p.After("Skills:").SkipWhitespaces();
            if (!prefix) return Error(prefix);

            List<IReportNode> skills = new List<IReportNode>();

            if (!p.Match("none")) {
                while (!p.EOF) {
                    if (skills.Count > 0) {
                        if (!Mem(p.After(",").SkipWhitespaces())) return Error(LastResult);
                    }

                    var skill = skillParser.Parse(p);
                    if (!skill) return Error(skill);

                    skills.Add(skill.Value);
                }
            }

            return Ok(ReportNode.Bag(
                ReportNode.Key("skills", ReportNode.Array(skills))
            ));
        }
    }
}
