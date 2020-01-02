using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

            Maybe<int> z = Maybe<int>.NA;
            Maybe<string> label = Maybe<string>.NA;
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
            p.PushBookmark();
            var word = p.Word();
            if (!word) return Error(word);

            int amount = 1;
            if (word.Match("unlimited")) {
                amount = -1;
                p.RemoveBookmark();
            }
            else {
                var intAmount = word.Integer();
                if (intAmount) {
                    amount = intAmount.Value;
                    p.RemoveBookmark();
                }
                else {
                    p.PopBookmark();
                }
            }

            Maybe<string> name = p.SkipWhitespaces().Before("[").SkipWhitespacesBackwards().AsString();
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
                ReportNode.Key("capacity", ReportNode.Object(
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

    public class UnitNameParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var name = p.Before("(").SkipWhitespacesBackwards().AsString();
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
            var name = p.Before("(").SkipWhitespacesBackwards().AsString();
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
        public UnitParser(SkillParser skillParser) {
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

                // there could be no flags
                var flagOrItem = p.Before(",", ".").RecoverWith(p);

                var item = itemParser.Parse(flagOrItem);
                if (item) {
                    items.Add(item.Value);
                }
                else {
                    flags.Add(flagOrItem.Reset().AsString());
                }
            }

            var remainingItems = p.Before(".").RecoverWith(p).Value;
            while (!remainingItems.EOF) {
                var nextItem = remainingItems.After(",").SkipWhitespaces();
                if (!nextItem) break;

                var item = itemParser.ParseMaybe(nextItem.Before(",").RecoverWith(nextItem));
                if (item) items.Add(item.Value);
            }

            List<IReportNode> props = new List<IReportNode>();
            while (!p.EOF) {
                var nextProp = p.After(".").SkipWhitespaces();
                if (!nextProp) break;

                var notSuccess = !nextProp.Success;

                nextProp = nextProp.Before(".").RecoverWith(nextProp);
                var prop = nextProp.OneOf(
                    weightParser,
                    capacityParser,
                    canStudyParser,
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

    // War 1
    // Trade 1
    // Magic 1
    public class FactionArgumentParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var key = p.BeforeBackwards(" ").SkipWhitespaces().AsString();
            if (!key) return Error(key);

            var value = p.SkipWhitespaces().Integer();
            if (!value) return Error(value);

            return Ok(ReportNode.Object(
                ReportNode.Str("key", key),
                ReportNode.Int("value", value)
            ));
        }
    }

    public class ReportFactionParser : ReportParser {
        public ReportFactionParser(FactionNameParser factionName, FactionArgumentParser argumentParser) {
            this.factionName = factionName;
            this.argumentParser = argumentParser;
        }

        private readonly FactionNameParser factionName;
        private readonly FactionArgumentParser argumentParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
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

    public class ReportDateParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var month = p.Before(",").AsString();
            if (!month) return Error(month);

            var year = p.After("Year").SkipWhitespaces().Integer();
            if (!year) return Error(year);

            return Ok(ReportNode.Bag(
                ReportArray.Key("date", ReportNode.Object(
                    ReportNode.Str("month", month),
                    ReportNode.Int("year", year)
                ))
            ));
        }
    }

    public class FactionStatusItemParser : ReportParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var key = p.Before(":").AsString();
            if (!key) return Error(key);

            var amount = p.After(":").SkipWhitespaces().Integer();
            if (!amount) return Error(amount);

            var max = p.After("(").Integer();
            if (!max) return Error(max);

            return Ok(ReportNode.Object(
                ReportNode.Str("key", key),
                ReportNode.Int("amount", amount),
                ReportNode.Int("max", max)
            ));
        }
    }

    // forest (50,22) in Mapa, contains Sembury [village], 5866 peasants (high elves), $2698.
    // ocean (2,6) in Atlantis Ocean.
    public class RegionHeaderParser : ReportParser {
        public RegionHeaderParser(CoordsParser coordsParser) {
            this.coordsParser = coordsParser;
        }

        private readonly CoordsParser coordsParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            var terrain = p.Before("(").SkipWhitespacesBackwards().AsString();
            if (!terrain) return Error(terrain);

            var coords = coordsParser.Parse(p);
            if (!coords) return Error(coords);

            var province = p.After("in").SkipWhitespaces().OneOf(
                x => x.Before(","),
                x => x.Before(".")
            ).AsString();
            if (!province) return Error(province);

            var settlement = p.Try(x => {
                var name = p.After(",").After("contains").SkipWhitespaces().Before("[").SkipWhitespacesBackwards().AsString();
                if (!name) return name.Convert<(string, string)>();

                var size = p.Between("[", "]").AsString();
                if (!size) return size.Convert<(string, string)>();

                return new Maybe<(string, string)>((name.Value, size.Value));
            });

            var population = p.Try(x => {
                var amount = p.After(",").SkipWhitespaces().Integer();
                if (!amount) return amount.Convert<(int, string)>();

                var race = p.Between("(", ")").AsString();
                if (!race) return race.Convert<(int, string)>();

                return new Maybe<(int, string)>((amount.Value, race.Value));
            });

            var tax = p.Try(x => x.After("$").Integer());

            return Ok(ReportNode.Bag(
                ReportNode.Str("terrain", terrain),
                ReportNode.Key("coords", coords.Value),
                ReportNode.Str("province", province),
                settlement ? ReportNode.Key("settlement", ReportNode.Object(
                    ReportNode.Str("name", settlement.Value.Item1),
                    ReportNode.Str("size", settlement.Value.Item2)
                )) : null,
                population ? ReportNode.Key("population", ReportNode.Object(
                    ReportNode.Int("amount", population.Value.Item1),
                    ReportNode.Str("race", population.Value.Item2)
                )) : null,
                tax ? ReportNode.Int("tax", tax) : null
            ));
        }
    }
}
