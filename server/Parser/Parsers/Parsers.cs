using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace atlantis {

    // high elf [HELF]
    // 2 high elves [HELF]
    // unlimited high elves [HELF]
    // high elf [HELF] at $34
    // 2 high elves [HELF] at $48
    // unlimited high elves [HELF] at $75
    public class ItemParser : BaseParser {
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

    // Weight: 30
    public class UnitWeightParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var weight = p.After("Weight:").SkipWhitespaces().Integer();
            if (!weight) return Error(weight);

            return Ok(ReportNode.Bag(
                ReportNode.Int("weight", weight)
            ));
        }
    }

    // Capacity: 0/0/45/0
    public class UnitCapacityParser : BaseParser {
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
    public class UnitCanStudyParser : BaseParser {
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
    public class UnitSkillsParser : BaseParser {
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

    public class UnitNameParser : BaseParser {
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

    public class FactionNameParser : BaseParser {
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
    public class UnitParser : BaseParser {
        public UnitParser(SkillParser skillParser, ItemParser itemParser) {
            nameParser = new UnitNameParser();
            factionParser = new FactionNameParser();
            this.itemParser = itemParser;
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
                var flagOrItem = p.Before(",", ".").RecoverWith(() => p);

                var item = itemParser.Parse(flagOrItem);
                if (item) {
                    items.Add(item.Value);
                }
                else {
                    flags.Add(flagOrItem.Reset().AsString());
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
    public class FactionArgumentParser : BaseParser {
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

    public class ReportFactionParser : BaseParser {
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

    public class ReportDateParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var month = p.Before(",").AsString();
            if (!month) return Error(month);

            var year = p.After("Year").SkipWhitespaces().Integer();
            if (!year) return Error(year);

            return Ok(ReportNode.Bag(
                ReportNode.Key("date", ReportNode.Object(
                    ReportNode.Int("month", MonthToNumber(month)),
                    ReportNode.Int("year", year)
                ))
            ));
        }

        private int MonthToNumber(string monthName) {
            switch (monthName.ToLowerInvariant()) {
                case "january": return 1;
                case "february": return 2;
                case "march": return 3;
                case "april": return 4;
                case "may": return 5;
                case "june": return 6;
                case "july": return 7;
                case "august": return 8;
                case "september": return 9;
                case "october": return 10;
                case "november": return 11;
                case "december": return 12;
            }

            throw new ArgumentOutOfRangeException();
        }
    }

    public class FactionStatusItemParser : BaseParser {
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
    public class RegionHeaderParser : BaseParser {
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

    public class RegionPropsParser : BaseParser {
        public RegionPropsParser(ItemParser itemParser) {
            this.itemParser = itemParser;
        }

        private readonly ItemParser itemParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            Maybe<IReportNode> ParaseItems(TextParser src, IReportParser parser, List<IReportNode> items) {
                if (src.Match("none")) {
                    return null;
                }

                while (!src.EOF) {
                    if (items.Count > 0) {
                        Maybe<TextParser> result = src.After(",").SkipWhitespaces();
                        if (!result) {
                            return Error(result);
                        }
                    }

                    var item = itemParser.Parse(src);
                    if (!item) {
                        return Error(item);
                    }

                    items.Add(item.Value);
                }

                return null;
            }

            p.SkipChar('-');

            Maybe<double> wages = null;
            Maybe<int> totalWages = null;
            List<IReportNode> wanted = new List<IReportNode>();
            List<IReportNode> forSale = new List<IReportNode>();
            Maybe<int> entertainment = null;
            List<IReportNode> products = new List<IReportNode>();

            while (!p.EOF) {
                var prop = p.SkipWhitespaces().Before(":").AsString();
                if (!prop) return Error(prop);

                p.After(":").SkipWhitespaces();

                Maybe<TextParser> value;
                switch (prop.Value) {
                    case "Wages": {
                        value = p.OneOf(
                            x => x.Before(")."),
                            x => x.Match("$0.")
                        )
                        .SkipWhitespaces();
                        break;
                    }

                    default: {
                        value = p.Before(".").SkipWhitespaces();
                        break;
                    }
                }
                if (!value) {
                    return Error(value);
                }

                p.After(".");

                switch (prop.Value) {
                    case "Wages": {
                        wages = value.Value.EOF
                            ? new Maybe<double>(0)
                            : value.Seek(1).Real();
                        if (!wages) {
                            return Error(wages);
                        }

                        if (value.After("(Max:").SkipWhitespaces()) {
                            totalWages = value.Seek(1).Integer();
                            if (!totalWages) {
                                return Error(wages);
                            }
                        }

                        break;
                    }

                    case "Wanted": {
                        var err = ParaseItems(value.Value, itemParser, wanted);
                        if (err != null) {
                            return err;
                        }
                        break;
                    }

                    case "For Sale": {
                        var err = ParaseItems(value.Value, itemParser, forSale);
                        if (err != null) {
                            return err;
                        }
                        break;
                    }

                    case "Products": {
                        var err = ParaseItems(value.Value, itemParser, products);
                        if (err != null) {
                            return err;
                        }
                        break;
                    }

                    case "Entertainment available": {
                        entertainment = value.Seek(1).Integer();
                        if (!entertainment) {
                            return Error(entertainment);
                        }
                        break;
                    }

                    default: {
                        continue;
                    }
                }
            }

            return Ok(ReportNode.Bag(
                ReportNode.Real("wages", wages?.Value ?? 0),
                ReportNode.Int("totalWages", totalWages?.Value ?? 0),
                ReportNode.Key("wanted", ReportNode.Array(wanted)),
                ReportNode.Key("forSale", ReportNode.Array(forSale)),
                ReportNode.Int("entertainment", entertainment?.Value ?? 0),
                ReportNode.Key("products", ReportNode.Array(products))
            ));
        }
    }

    public class RegionExistsParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            throw new NotImplementedException();
        }
    }

    // + Building [5] : Stables; Imperial Stables.
    // + AE Mayfield [286] : Fleet, 2 Cogs; Load: 485/1000; Sailors: 12/12; MaxSpeed: 4.
    // + AE Mayfield [286] : Fleet, 2 Cogs, 1 Longship; Load: 485/1000; Sailors: 12/12; MaxSpeed: 4.
    // + AE Empire [246] : Fleet, 10 Corsairs; Load: 5593/10000; Sailors: 135/150; MaxSpeed: 0; Sail directions: S, SW.
    // + AE Triangulum [329] : Fleet, 3 Corsairs; Sail directions: S, SW; Shiny new corsairs ready to engage any enemy. Built bay Imperial Shipyards..
    // + Shaft [1] : Shaft, contains an inner location.
    // + Lair [1] : Lair, closed to player units.
    // + Tower [1] : Tower, needs 10.
    // + Tower [1] : Tower, engraved with Runes of Warding.
    // + Ruin [1] : Ruin, closed to player units.
    // + The Kings Highway [1] : Road N.
    // + Trade Academy [NIMB] [Nort Triders] [2] : Tower; comment.

    // + {Name} [{Number}] : {Type}, {Int} {Type}, {Flag}; {Key}: {Value}; {Description}.
    public class StructureParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var nameAndNumber = p.After("+").SkipWhitespaces().Before("] : ");  // lets hope noone will use this combination in their building names
            if (!nameAndNumber) return Error(nameAndNumber);

            var name = nameAndNumber.BeforeBackwards("[").SkipWhitespacesBackwards().AsString();
            if (!name) return Error(name);

            var number = nameAndNumber.After("[").Integer();
            if (!number) return Error(number);

            p.After("] :").SkipWhitespaces();

            Queue<TextParser> props = new Queue<TextParser>();
            if (!p.EOF) {
                var props1 = p
                    .Before(";")
                    .RecoverWith(() => p.BeforeBackwards("."))
                    .List(",", item => item.SkipWhitespaces());

                var props2 = p
                    .After(";")
                    .BeforeBackwards(".")
                    .List(";", item => item.SkipWhitespaces());

                if (props1)
                    foreach (var prop in props1.Value)
                        props.Enqueue(prop);

                if (props2)
                    foreach (var prop in props2.Value)
                        props.Enqueue(prop);
            }

            var type = props.Dequeue().AsString();

            var structure = ReportNode.Object(
                ReportNode.Str("name", name),
                ReportNode.Int("number", number),
                ReportNode.Str("type", type)
            );

            if (type.Equals("fleet", StringComparison.OrdinalIgnoreCase)) {
                var contents = ReportNode.Array();
                structure.Add(ReportNode.Key("contents", contents));

                while (props.Count > 0) {
                    var item = props.Peek();
                    item.PushBookmark();

                    var objCount = item.Integer();
                    var objType = item.SkipWhitespaces().AsString();

                    if (!objCount || !objType) {
                        item.PopBookmark();
                        break;
                    }
                    else {
                        contents.Add(ReportNode.Object(
                            ReportNode.Int("count", objCount),
                            ReportNode.Str("type", objType)
                        ));
                        props.Dequeue();
                    }
                }
            }

            var flags = ReportNode.Array();
            structure.Add(ReportNode.Key("flags", flags));

            List<TextParser> unknownProps = new List<TextParser>();
            foreach (var prop in props) {
                var knownProp = prop.OneOf(
                    x => x.After("needs")
                        .SkipWhitespaces()
                        .Integer()
                        .Map(v => ReportNode.Int("needs", v)),
                    x => {
                        var targetProp = x.After("Load:").SkipWhitespaces();
                        if (!targetProp) return targetProp.Convert<IReportNode>();

                        var value = targetProp.Before("/").Integer();
                        if (!value) return value.Convert<IReportNode>();

                        var max = targetProp.After("/").Integer();
                        if (!max) return max.Convert<IReportNode>();

                        return new Maybe<IReportNode>(ReportNode.Key("load", ReportNode.Object(
                            ReportNode.Int("used", value),
                            ReportNode.Int("max", max)
                        )));
                    },
                    x => {
                        var targetProp = x.After("Sailors:").SkipWhitespaces();
                        if (!targetProp) return targetProp.Convert<IReportNode>();

                        var value = targetProp.Before("/").Integer();
                        if (!value) return value.Convert<IReportNode>();

                        var max = targetProp.After("/").Integer();
                        if (!max) return max.Convert<IReportNode>();

                        return new Maybe<IReportNode>(ReportNode.Key("sailors", ReportNode.Object(
                            ReportNode.Int("current", value),
                            ReportNode.Int("required", max)
                        )));
                    },
                    x => x.After("MaxSpeed:")
                        .SkipWhitespaces()
                        .Integer()
                        .Map(v => ReportNode.Int("speed", v)),
                    x => x.After("Sail directions:")
                        .SkipWhitespaces()
                        .List(",", item => item.SkipWhitespaces())
                        .Map(v => ReportNode.Key("sailDirections",ReportNode.Array(
                            v.Select(sd => ReportNode.Str(sd.AsString()))
                        )))
                );
                if (knownProp) {
                    structure.Add(knownProp.Value);
                    continue;
                }

                var knownFlag = prop.OneOf(
                    x => x.Match("closed to player units"),
                    x => x.Match("contains an inner location"),
                    x => x.Match("engraved with runes of warding")
                ).AsString();
                if (knownFlag) {
                    flags.Add(ReportNode.Str(knownFlag.Value));
                    continue;
                }

                unknownProps.Add(prop);
            }

            if (unknownProps.Count > 0) {
                structure.Add(ReportNode.Str("description", string.Join("; ", unknownProps.Select(x => x.AsString()))));
            }

            return Ok(structure);
        }
    }
}
