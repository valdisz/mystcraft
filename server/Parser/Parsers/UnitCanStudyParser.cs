using System.Collections.Generic;

namespace advisor
{
    // Can Study: endurance [ENDU]
    // Can Study: fire [FIRE], earthquake [EQUA], force shield [FSHI], energy shield [ESHI], spirit shield [SSHI], magical healing [MHEA], gate lore [GATE], farsight [FARS], mind reading [MIND], weather lore [WEAT], wolf lore [WOLF], necromancy [NECR], demon lore [DEMO], phantasmal entertainment [PHEN], create phantasmal beasts [PHBE], create phantasmal undead [PHUN], create phantasmal demons [PHDE], invisibility [INVI], true seeing [TRUE], dispel illusions [DISP], enchant swords [ESWO], enchant armor [EARM], enchant shields [ESHD], create cornucopia [CRCO], transmutation [TRNS]
    public class UnitCanStudyParser : BaseParser {
        public UnitCanStudyParser(IParser skillParser) {
            this.skillParser = skillParser;
        }

        private readonly IParser skillParser;

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            var prefix = p.After("Can Study:").SkipWhitespaces();
            if (!prefix) return Error(prefix);

            List<IReportNode> skills = new List<IReportNode>();

            while (!p.EOF) {
                if (skills.Count > 0) {
                    if (!Mem(p.After(",").SkipWhitespaces())) return Error(LastResult);
                }

                var skill = skillParser.Parse(p);
                if (!skill) return Error(skill);

                skills.Add(skill.Value);
            }

            return Ok(ReportNode.Bag(
                ReportNode.Key("canStudy", ReportNode.Array(skills))
            ));
        }
    }
}
