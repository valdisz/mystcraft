namespace advisor {
    using System.Collections.Generic;
    using System.Linq;

    // Sandlings (384), 7 sandlings [SAND] (Combat 3/3, Attacks 2, Hits 2, Tactics 1).
    // Nookers (12462), Nomads (82), behind, 14 gnolls [GNOL], 13 humans [HUMN], combat 1.
    // for nookers (8466), Nomads (82), behind, 5 humans [HUMN], combat 1.
    public class BattleUnitParser : BaseParser {
        readonly IParser nameParser = new UnitNameParser();
        readonly IParser factionParser = new FactionNameParser();
        readonly IParser itemParser = new ItemParser();
        readonly IParser skillParser = new BattleSkillParser();

        protected override Maybe<IReportNode> Execute(TextParser p) {
            // don't need dot at the end of line
            p = p.BeforeBackwards(".");

            // unit name
            // Legatus Legionis Marcus (640)
            var name = nameParser.Parse(p);
            if (!name) return Error(name);

            // optional faction name
            // , Avalon Empire (15)
            var faction = p.Try(parser => factionParser.Parse(parser.After(",").SkipWhitespaces()));

            // now check for description, it must start with `;`
            Maybe<string> description = Maybe<string>.NA;
            p.PushBookmark();
            if (p.After(";")) {
                var descPos = p.Pos - 2;
                description = new Maybe<string>(p.SkipWhitespaces().SkipWhitespacesBackwards().AsString());

                // w/o description
                p.PopBookmark();
                p = p.Slice(descPos - p.Pos + 1);
            }
            else {
                p.RemoveBookmark();
            }

            List<string> flags = new List<string>();
            List<IReportNode> items = new List<IReportNode>();
            List<IReportNode> skills = new List<IReportNode>();

            // all element after faction name till first item are flags
            while (!p.EOF) {
                // , flag
                if (!Mem(p.After(",").SkipWhitespaces())) return Error(LastResult);

                var item = p.Try(_ => itemParser.Parse(_));
                if (item) {
                    items.Add(item.Value);
                    continue;
                }

                var skill = p.Try(_ => skillParser.Parse(_));
                if (skill) {
                    skills.Add(skill.Value);
                    continue;
                }

                var flag = p.Before(",", ".").RecoverWith(() => p).AsString();
                flags.Add(flag);
            }

            var result = ReportNode.Object();
            result.Add(name.Value);
            result.Add(ReportNode.Key("faction", faction ? ReportNode.Object(faction.Value) : ReportNode.Null()));
            result.Add(ReportNode.Key("description", description ? ReportNode.Str(description) : ReportNode.Null()));
            result.Add(ReportNode.Key("flags", ReportNode.Array(flags.Select(x => ReportNode.Str(x)))));
            result.Add(ReportNode.Key("items", ReportNode.Array(items)));
            result.Add(ReportNode.Key("skills", ReportNode.Array(skills)));

            return new Maybe<IReportNode>(result);
        }
    }
}
