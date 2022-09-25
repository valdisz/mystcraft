namespace advisor {
    using System.Collections.Generic;
    using System.Linq;

    public class UnitReadyItemParser : BaseParser {
        public UnitReadyItemParser(ItemParser itemParser) {
            this.itemParser = itemParser;
        }

        private readonly ItemParser itemParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            var prefix = p.After("Ready item:").SkipWhitespaces();
            if (!prefix) return Error(prefix);

            var item = itemParser.Parse(p);
            if (!item) return Error(item);

            return Ok(ReportNode.Bag(
                ReportNode.Key("readyItem", item.Value)
            ));
        }
    }

    public class UnitCombatSpellParser : BaseParser {
        public UnitCombatSpellParser(IParser skillParser) {
            this.skillParser = skillParser;
        }

        private readonly IParser skillParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            var prefix = p.After("Combat spell:").SkipWhitespaces();
            if (!prefix) return Error(prefix);

            var skill = skillParser.Parse(p);
            if (!skill) return Error(skill);

            return Ok(ReportNode.Bag(
                ReportNode.Key("combatSpell", skill.Value)
            ));
        }
    }

    /*
* Legatus Legionis Marcus (640),
on guard,
Avalon Empire (15),
behind, weightless battle spoils,
leader [LEAD], 2 books of exorcism [BKEX], boots of levitation [BOOT], 3 magic carpets [CARP],
leather armor [LARM], amulet of protection [AMPR], winged horse [WING], 180 silver [SILV].
Weight: 61.
Capacity: 115/115/130/15.
Skills: tactics [TACT] 5 (450), observation [OBSE] 2 (160), stealth [STEA] 2 (90), manipulation [MANI] 3 (185).
Ready item: book of exorcism [BKEX].
    */
    public class UnitParser : BaseParser {
        public UnitParser(SkillParser skillParser, ItemParser itemParser) {
            nameParser = new UnitNameParser();
            factionParser = new FactionNameParser();
            this.itemParser = itemParser;
            skillsParser = new UnitSkillsParser(skillParser);
            weightParser = new UnitWeightParser();
            capacityParser = new UnitCapacityParser();
            canStudyParser = new UnitCanStudyParser(skillParser);
            readyItemParser = new UnitReadyItemParser(itemParser);
            combatSpellParser = new UnitCombatSpellParser(skillParser);
        }

        readonly IParser nameParser;
        readonly IParser factionParser;
        readonly IParser itemParser;
        readonly IParser skillsParser;
        readonly IParser weightParser;
        readonly IParser capacityParser;
        readonly IParser canStudyParser;
        readonly IParser readyItemParser;
        readonly IParser combatSpellParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            // don't need dot at the end of line
            p = p.BeforeBackwards(".");

            bool own = false;
            if (p.Match("*")) {
                own = true;
            }
            else if (!Mem(p.Match("-"))) {
                return Error(LastResult);
            }

            p = p.SkipWhitespaces();

            // unit name
            // Legatus Legionis Marcus (640)
            var name = nameParser.Parse(p);
            if (!name) return Error(name);

            // optional on guard flag
            // , on guard
            var onGuard = p.Try(parser => parser.After(",").SkipWhitespaces().Match("on guard"));

            // optional faction name unless this is own unit
            // , Avalon Empire (15)
            var faction = p.Try(parser => factionParser.Parse(parser.After(",").SkipWhitespaces()));
            if (!faction && own) {
                return Error(faction);
            }

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

            // all element after faction name till first item are flags
            while (items.Count == 0 && !p.EOF) {
                // , flag
                if (!Mem(p.After(",").SkipWhitespaces())) return Error(LastResult);

                var item = p.Try(_ => itemParser.Parse(_));
                if (item) {
                    items.Add(item.Value);
                }
                else {
                    var flag = p.Before(",", ".").RecoverWith(() => p).AsString();
                    flags.Add(flag);
                }
            }

            var remainingItems = p.Before(".").RecoverWith(() => p).Value;
            while (!remainingItems.EOF) {
                var nextItem = remainingItems.After(",").SkipWhitespaces();
                if (!nextItem) break;

                var item = itemParser.ParseMaybe(nextItem.Before(",").RecoverWith(() => nextItem));
                if (item) items.Add(item.Value);
            }

            List<IReportNode> props = new List<IReportNode>();
            while (!p.EOF) {
                var nextProp = p.After(".").SkipWhitespaces();
                if (!nextProp) break;

                var notSuccess = !nextProp.Success;

                nextProp = nextProp.Before(".").RecoverWith(() => nextProp);
                var prop = nextProp.OneOf(
                    weightParser,
                    capacityParser,
                    canStudyParser,
                    readyItemParser,
                    combatSpellParser,
                    skillsParser
                );
                if (prop) props.Add(prop.Value);
            }

            var result = ReportNode.Object();
            result.Add(ReportNode.Bool("own", own));
            result.Add(name.Value);
            result.Add(ReportNode.Key("faction", faction ? ReportNode.Object(faction.Value) : ReportNode.Null()));
            result.Add(ReportNode.Key("description", description ? ReportNode.Str(description) : ReportNode.Null()));
            result.Add(ReportNode.Bool("onGuard", onGuard));
            result.Add(ReportNode.Key("flags", ReportNode.Array(flags.Select(x => ReportNode.Str(x)))));
            result.Add(ReportNode.Key("items", ReportNode.Array(items)));
            result.AddRange(props);

            return new Maybe<IReportNode>(result);
        }
    }
}
