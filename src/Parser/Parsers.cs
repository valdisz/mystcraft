using System;
using System.Collections.Generic;
using System.Linq;

namespace atlantis {
    // (12,34)
    // (12,34,2)
    // (12,34,2 <underworld>)
    public class CoordsParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            if (!Mem(p.Between("(", ")"))) return Error(LastResult);
            var content = LastResult.Value;

            Maybe<int> x = content.SkipWhitespaces().Integer();
            if (!x) return Error(x);

            Maybe<int> y = content.SkipWhitespaces().After(",").SkipWhitespaces().Integer();
            if (!y) return Error(y);

            Maybe<int> z = null;
            Maybe<string> label = null;
            if (content.SkipWhitespaces().After(",")) {
                z = content.SkipWhitespaces().Integer();
                content.SkipWhitespaces();
                label = content.Between("<", ">").AsString();
            }

            content.Try(parser => {
                if (parser.SkipWhitespaces().After(",")) {
                    z = parser.SkipWhitespaces().Integer();
                    parser.SkipWhitespaces();
                    label = parser.Between("<", ">").AsString();

                    return new Maybe<object>(null);
                }

                return Maybe<object>.NA;
            });

            return Ok(ReportNode.Object(
                ReportNode.Int("x", x),
                ReportNode.Int("y", y),
                z ? ReportNode.Int("z", z) : null,
                label ? ReportNode.Str("label", label) : null
            ));
        }
    }

    // high elf [HELF]
    // 2 high elves [HELF]
    // unlimited high elves [HELF]
    // high elf [HELF] at $34
    // 2 high elves [HELF] at $48
    // unlimited high elves [HELF] at $75
    public class ItemParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var word = p.Word();
            if (!word) return Error(word);

            Maybe<int> amount = word.Match("unlimited")
                ? new Maybe<int>(-1)
                : word.Integer();
            if (!amount) {
                amount = new Maybe<int>(1);
            }

            Maybe<string> name = p.SkipWhitespaces().Before("[").AsString();
            if (!name) return Error(name);

            Maybe<string> code = p.Between("[", "]").AsString();
            if (!code) return Error(code);

            Maybe<int> price = p.Try(parser => parser.SkipWhitespaces().Word().Match("at")
                ? p.SkipWhitespaces().Word().Seek(1).Integer()
                : Maybe<int>.NA
            );

            return Ok(ReportNode.Object(
                ReportNode.Int("amount", amount),
                ReportNode.Str("name", name),
                ReportNode.Str("code", code),
                price ? ReportNode.Int("price", price) : null
            ));
        }
    }

    // combat [COMB] 1 (31)
    public class SkillParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var name = p.Before("[").AsString();
            if (!name) return Error(name);

            var code = p.Between("[", "]").AsString();
            if (!code) return Error(code);

            var levelAndDays = p.Try(parser => {
                var level = parser.SkipWhitespaces().Integer();
                var days = level
                    ? parser.SkipWhitespaces().Between("(", ")").Integer()
                    : Maybe<int>.NA;

                return level && days
                    ? new Maybe<(int level, int days)>((level.Value, days.Value))
                    : Maybe<(int level, int days)>.NA;
            });

            return Ok(ReportNode.Object(
                ReportNode.Str("name", name),
                ReportNode.Str("code", code),
                levelAndDays ? ReportNode.Int("level", levelAndDays.Value.level) : null,
                levelAndDays ? ReportNode.Int("days", levelAndDays.Value.days) : null
            ));
        }
    }

    // Weight: 30
    public class UnitWeightParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var weight = p.After("Weight:").SkipWhitespaces().Integer();
            if (!weight) return Error(weight);

            return Ok(ReportNode.Bag(
                ReportNode.Int("weight", weight)
            ));
        }
    }

    // Capacity: 0/0/45/0
    public class UnitCapacityParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var capacities = p.After("Capacity:").SkipWhitespaces();
            if (!capacities) return Error(capacities);

            var flying = capacities.Integer();
            if (!flying) return Error(flying);

            var riding = capacities.After("/").Integer();
            if (!riding) return Error(riding);

            var walking = capacities.After("/").Integer();
            if (!walking) return Error(walking);

            var swimming = capacities.After("/").Integer();
            if (!swimming) return Error(swimming);

            return Ok(ReportNode.Bag(
                ReportNode.Key("weight", ReportNode.Object(
                    ReportNode.Int("flying", flying),
                    ReportNode.Int("riding", riding),
                    ReportNode.Int("walking", walking),
                    ReportNode.Int("swimming", swimming)
                ))
            ));
        }
    }

    // Can Study: endurance [ENDU]
    // Can Study: fire [FIRE], earthquake [EQUA], force shield [FSHI], energy shield [ESHI], spirit shield [SSHI], magical healing [MHEA], gate lore [GATE], farsight [FARS], mind reading [MIND], weather lore [WEAT], wolf lore [WOLF], necromancy [NECR], demon lore [DEMO], phantasmal entertainment [PHEN], create phantasmal beasts [PHBE], create phantasmal undead [PHUN], create phantasmal demons [PHDE], invisibility [INVI], true seeing [TRUE], dispel illusions [DISP], enchant swords [ESWO], enchant armor [EARM], enchant shields [ESHD], create cornucopia [CRCO], transmutation [TRNS]
    public class UnitCanStudyParser : ReportParser {
        public UnitCanStudyParser(IReportParser skillParser) {
            this.skillParser = skillParser;
        }

        private readonly IReportParser skillParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
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

    // Skills: endurance [ENDU] 1 (30)
    // Skills: fire [FIRE] 1 (30), earthquake [EQUA] 1 (30)
    public class UnitSkillsParser : ReportParser {
        public UnitSkillsParser(IReportParser skillParser) {
            this.skillParser = skillParser;
        }

        private readonly IReportParser skillParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            var prefix = p.After("Skills:").SkipWhitespaces();
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
                ReportNode.Key("skills", ReportNode.Array(skills))
            ));
        }
    }

    public class UnitNameParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var name = p.Before("(").AsString();
            if (!name) return Error(name);

            var num = p.Between("(", ")").Integer();
            if (!num) return Error(num);

            return Ok(ReportNode.Bag(
                ReportNode.Str("name", name),
                ReportNode.Int("number", num)
            ));
        }
    }

    public class FactionNameParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var name = p.Before("(").AsString();
            if (!name) return Error(name);

            var num = p.Between("(", ")").Integer();
            if (!num) return Error(num);

            return Ok(ReportNode.Bag(
                ReportNode.Str("name", name),
                ReportNode.Int("number", num)
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
    public class UnitParser : ReportParser {
        public UnitParser() {
            var skillParser = new SkillParser();

            nameParser = new UnitNameParser();
            factionParser = new FactionNameParser();
            itemParser = new ItemParser();
            skillsParser = new UnitSkillsParser(skillParser);
            weightParser = new UnitWeightParser();
            capacityParser = new UnitCapacityParser();
            canStudyParser = new UnitCanStudyParser(skillParser);
        }

        readonly IReportParser nameParser;
        readonly IReportParser factionParser;
        readonly IReportParser itemParser;
        readonly IReportParser skillsParser;
        readonly IReportParser weightParser;
        readonly IReportParser capacityParser;
        readonly IReportParser canStudyParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            bool own = false;
            if (p.Match("*")) {
                own = true;
            }
            else if (!Mem(p.Match("-"))) {
                return Error(LastResult);
            }

            p.SkipWhitespaces();

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
                description = p.BeforeBackwards(".").AsString();
                if (!description) return Error(description);

                p.PopBookmark();
                p = p.Slice(descPos - p.Pos + 1);
            }
            else {
                p.PopBookmark();
            }

            List<string> flags = new List<string>();
            List<IReportNode> items = new List<IReportNode>();

            // all element after faction name till first item are flags
            do {
                // , flag
                if (!Mem(p.After(",").SkipWhitespaces())) return Error(LastResult);

                // there could be no flags
                var flagOrItem = p.Before(",", ".");
                if (!flagOrItem) return Error(flagOrItem);

                var item = itemParser.Parse(flagOrItem);
                if (item) {
                    items.Add(item.Value);
                }
                else {
                    flags.Add(flagOrItem.Reset().AsString());
                }
            } while (items.Count == 0);

            // all elements till `.` are items
            while (!p.Match(".")) {
                if (!Mem(p.After(",").SkipWhitespaces())) return Error(LastResult);
                var item = itemParser.Parse(p.Before(",", "."));
                if (!item) return Error(item);
                items.Add(item.Value);
            }

            p.SkipWhitespaces();

            List<IReportNode> props = new List<IReportNode>();
            while (!p.EOF) {
                Mem(p.Before("."));
                var elm = LastResult ? LastResult : p;

                var prop = elm.OneOf(
                    weightParser,
                    capacityParser,
                    canStudyParser,
                    skillsParser
                );
                if (prop) props.Add(prop.Value);

                p.Try(parser => parser.After(".")).SkipWhitespaces();
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
