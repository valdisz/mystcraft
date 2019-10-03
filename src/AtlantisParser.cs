namespace atlantis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Pidgin;
    using static Pidgin.Parser;
    using static Pidgin.Parser<char>;
    using static Tokens;
    using static Nodes;

    using ParserNode = Pidgin.Parser<char, IReportNode>;
    using ParserNone = Pidgin.Parser<char, Pidgin.Unit>;
    using System.Text;
    using System.IO;
    using System.Globalization;

    public interface IReportNode
    {
        string Type { get; }
        bool HasChildren { get; }
        IList<IReportNode> Children { get; }

        void Add(IReportNode child);
        void AddRange(IEnumerable<IReportNode> children);
        void AddRange(params IReportNode[] children);

        IReportNode[] ByType(string type);
    }

    public static class ReportNodeExtensions {
        public static string StrValueOf(this IReportNode node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return (nodes[0] as ValueReportNode<string>).Value;
        }

        public static int? IntValueOf(this IReportNode node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return (nodes[0] as ValueReportNode<int>).Value;
        }

        public static IReportNode FirstByType(this IReportNode node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return nodes[0];
        }
    }

    public abstract class BaseReportNode : IReportNode
    {
        public abstract string Type { get; }
        public abstract bool HasChildren { get; }
        public abstract IList<IReportNode> Children { get; }

        public virtual void Add(IReportNode child)
        {
            if (HasChildren) Children.Add(child);
        }

        protected virtual void AddMany(IEnumerable<IReportNode> children) {
            if (HasChildren) {
                foreach (var child in children) {
                    Children.Add(child);
                }
            }
        }

        public void AddRange(IEnumerable<IReportNode> children) => AddMany(children);
        public void AddRange(params IReportNode[] children)  => AddMany(children);

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Type);
            if (HasChildren) {
                foreach (var child in Children) {
                    using var childReader = new StringReader(child.ToString());
                    string line;
                    while ((line = childReader.ReadLine()) != null) {
                        sb.Append("  ");
                        sb.AppendLine(line);
                    }
                }
            }

            return sb.ToString();
        }

        public IReportNode[] ByType(string type)
        {
            if (!HasChildren) {
                throw new InvalidOperationException();
            }

            return Children.Where(x => x.Type == type).ToArray();
        }
    }

    public class ReportNode : BaseReportNode
    {
        public ReportNode(string type, params IReportNode[] nodes) {
            this.type = type;
            if (nodes != null && nodes.Length > 0) {
                children.AddRange(nodes);
            }
        }

        private readonly string type;
        private readonly List<IReportNode> children = new List<IReportNode>();

        public override string Type => type;
        public override bool HasChildren => true;
        public override IList<IReportNode> Children => children;
    }

    public class ValueReportNode<T> : BaseReportNode
    {
        public ValueReportNode(string type, T value) {
            this.type = type;
            Value = value;
        }

        private readonly string type;

        public override string Type => type;
        public override bool HasChildren => false;
        public override IList<IReportNode> Children => throw new NotImplementedException();

        public T Value { get; }

        public override string ToString() => $"{Type} ({Value})";
    }

    public static class Nodes {
        public static IReportNode Node(string type, params IReportNode[] children) {
            return new ReportNode(type, children);
        }

        public static IReportNode Node(string type, IEnumerable<IReportNode> children) {
            return new ReportNode(type, (children ?? Enumerable.Empty<IReportNode>()).ToArray());
        }

        public static IReportNode Str(string type, string value) {
            return new ValueReportNode<string>(type, value.Trim());
        }

        public static IReportNode Num(string type, int value) {
            return new ValueReportNode<int>(type, value);
        }

        public static IReportNode Num(string type, string value) {
            return new ValueReportNode<int>(type, int.Parse(value));
        }

        public static IReportNode Float(string type, double value) {
            return new ValueReportNode<double>(type, value);
        }

        public static IReportNode Float(string type, string value) {
            return new ValueReportNode<double>(type, double.Parse(value, CultureInfo.InstalledUICulture));
        }


        public static ParserNode Node(this Parser<char, IEnumerable<IReportNode>> parser, string type) {
            return parser.Select(x => Node(type, x));
        }

        public static ParserNode Node(this Parser<char, IEnumerable<Maybe<IReportNode>>> parser, string type) {
            return parser.Select(x => Node(type, x.Where(node => node.HasValue).Select(node => node.Value)));
        }

        public static ParserNode Str(this Parser<char, string> parser, string type) {
            return parser.Select(x => Str(type, x));
        }

        public static ParserNode Num(this Parser<char, string> parser, string type) {
            return parser.Select(x => Num(type, x));
        }

        public static ParserNode Num(this Parser<char, int> parser, string type) {
            return parser.Select(x => Num(type, x));
        }

        public static ParserNode Float(this Parser<char, string> parser, string type) {
            return parser.Select(x => Float(type, x));
        }

        public static ParserNode Float(this Parser<char, double> parser, string type) {
            return parser.Select(x => Float(type, x));
        }
    }

    public static class AtlantisParser {
        public static readonly ParserNone ReportHeader =
            String("Atlantis Report For:").IgnoreResult();

        // War 1
        public static readonly ParserNode FactionAttribute =
            Token(AtlantisCharset().Exclude(Charset.List(' ', ',')))
                .AtLeastOnceString()
                .Separated(SkipWhitespaces)
                .Where(s => s.Count() > 1)
                .Select(s => Node("attribute",
                    Str("key", string.Join(" ", s.SkipLast(1))),
                    Num("value", s.Last()))
                );

        // Avalon Empire
        public static readonly ParserNode FactionName =
            TText(AtlantisCharset())
                .Str("name");

        // (15)
        public static readonly ParserNode FactionNumber =
            Int(10).BetweenParenthesis().Num("number");

        public static readonly ParserNode Faction =
            Sequence(
                FactionName,
                FactionNumber
            )
            .Node("faction");

        // (War 1, Trade 2, Magic 2)
        public static readonly ParserNode FactionAttributes =
            FactionAttribute
                .Separated(TCommaWp)
                .BetweenParenthesis()
                .Node("attributes");

        // Avalon Empire (15) (War 1, Trade 2, Magic 2)
        public static readonly ParserNode ReportFaction =
            Sequence(
                Faction.Before(SkipWhitespaces),
                FactionAttributes
            )
            .Node("faction-info");

        // May, Year 3
        public static readonly ParserNode ReportDate =
            Sequence(
                TText(AtlantisCharset(), stopBefore: TCommaWp.IgnoreResult())
                    .Str("month"),
                String("Year")
                    .Then(SkipWhitespaces)
                    .Then(Int(10))
                    .Num("year")
            )
            .Node("date");

/*
Declared Attitudes (default Unfriendly):
Hostile : none.
Unfriendly : Creatures (2).
Neutral : none.
Friendly : Semigallians (18), Disasters Inc (43).
Ally : none.
*/
        public static ParserNode Stance =
            OneOf(
                Try(String("Hostile")),
                Try(String("Unfriendly")),
                Try(String("Neutral")),
                Try(String("Friendly")),
                String("Ally")
            )
            .Str("stance");

        // Declared Attitudes (default Neutral):
        public static ParserNode DefaultAttitude =
            String("Declared Attitudes (default ")
                .Then(Stance)
                .Before(String("):"))
                .Select(attitude => Node("default", attitude));


        public static Parser<char, IEnumerable<IReportNode>> ListOf(ParserNode itemParser, Parser<char, Pidgin.Unit> separator = null) {
            separator = separator ?? TCommaWp.IgnoreResult();

            Parser<char, IEnumerable<IReportNode>> recItemParser = null;
            recItemParser = itemParser
                .Then(
                    Rec(() => separator.Then(recItemParser)).Optional(),
                    MakeSequence
                );

            return OneOf(
                Try(String("none").WithResult(Enumerable.Empty<IReportNode>())),
                recItemParser
            );
        }

        public static ParserNode ListOfFactions =
            ListOf(Faction)
                .Before(TDot.Then(EndOfLine))
                .Node("factions");

        public static ParserNode Attitudes =
            DefaultAttitude
                .Before(EndOfLine)
                .Then(
                    Sequence(
                        Stance
                            .Before(TColon.Between(SkipWhitespaces)),
                        ListOfFactions
                    )
                    .Node("attitude")
                    .Repeat(5),
                    (def, list) => MakeSequence(def, list)
                )
                .Node("attitudes");

        // (0,0,2 <underworld>)
        public static readonly ParserNode Coords =
            Sequence(
                Int(10)
                    .Num("x")
                    .LikeOptional(),
                TComma
                    .Then(Int(10))
                    .Num("y")
                    .LikeOptional(),
                TComma
                    .Then(Int(10))
                    .Num("z")
                    .Optional(),
                SkipWhitespaces
                    .Then(TText(AtlantisCharset().Exclude(Charset.List('<', '>'))).Between(Char('<'), Char('>')))
                    .Str("label")
                    .Optional()
            )
            .BetweenParenthesis()
            .Node("coords");

        // underforest (0,0,2 <underworld>) in Ryway
        public static readonly ParserNode Location =
            Sequence(
                TText(AtlantisCharset(simpleWhitepsace: true).Exclude(Charset.List(':')))
                    .Str("terrain"),
                Coords,
                SkipWhitespaces
                    .Then(String("in"))
                    .Then(SkipWhitespaces)
                    .Then(TText(AtlantisCharset().Exclude(Charset.List(',', '.'))))
                    .Str("prvonice")
            )
            .Node("location");

        // [town]
        public static readonly ParserNode SettlementSize =
            TText(AtlantisCharset().Exclude(Charset.List('[', ']')))
                .BetweenBrackets()
                .Str("size");

        // contains Sinsto [town]
        public static readonly ParserNode Settlement =
            String("contains")
                .Then(SkipWhitespaces)
                .Then(
                    Sequence(
                        TText(AtlantisCharset().Exclude(Charset.List(',', '.')), stopBefore: Lookahead(SettlementSize).IgnoreResult())
                            .Str("name"),
                        SettlementSize
                    )
                )
                .Node("settlement");

        // 1195 peasants (gnomes)
        public static readonly ParserNode Population =
            Sequence(
                Int(10).Num("amount"),
                String("peasants")
                    .Between(SkipWhitespaces)
                    .Then(TText(AtlantisCharset()).BetweenParenthesis())
                    .Str("race")
            )
            .Node("population");

        public static readonly ParserNode Tax =
            Char('$').Then(Int(10)).Num("tax");

        // underforest (0,0,2 <underworld>) in Ryway, 1195 peasants (gnomes), $621.
        // underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town], 10237 peasants (drow elves), $5937.
        public static readonly ParserNode Summary =
            Location
                .Then(
                    TCommaWp
                        .Then(
                            OneOf(Settlement, Population, Tax)
                        )
                        .Many()
                        .Optional(),
                    (loc, other) => new[] { loc }.Concat(other.GetValueOrDefault(Enumerable.Empty<IReportNode>()))
                )
                .Before(TDot.Then(EndOfLine))
                .Node("region-header");

        // The weather was clear last month; it will be clear next month.
        public static readonly ParserNone Weather =
            Whitespace.Repeat(2)
                .Then(String("The weather"))
                .Then(TText(AtlantisCharset()))
                .IgnoreResult();

        public static Parser<char, string> MultiLineText(
            Parser<char, Pidgin.Unit> prefix = null,
            Parser<char, Pidgin.Unit> nextLineIdent = null,
            Parser<char, Pidgin.Unit> firstLineMarker = null) {

            var line = Any.Until(EndOfLine).Select(string.Concat);
            nextLineIdent = nextLineIdent ?? Char(' ').Repeat(2).IgnoreResult();

            if (prefix != null) {
                line = prefix.Then(line);
                nextLineIdent = prefix.Then(nextLineIdent);
            }

            Parser<char, IEnumerable<string>> recLine = null;
            recLine = line
                .Then(
                    Rec(() => Try(nextLineIdent.Then(recLine)).Optional()),
                    MakeSequence
                );

            var parser = recLine.Select(s => string.Join(" ", s));

            return firstLineMarker != null
                ? firstLineMarker.Then(parser)
                : parser;
        }

        public static readonly ParserNone RegionAttributeDivider =
            TColon.Between(SkipWhitespaces).IgnoreResult();

        public static readonly ParserNone DotTerminator =
            TDot.Then(EndOfLine).IgnoreResult();

        public static readonly Parser<char, string> RegionAttribute =
            TText(
                AtlantisCharset(simpleWhitepsace: true).Exclude(Charset.List(':', '.')),
                stopBefore: Lookahead(Try(RegionAttributeDivider.Or(DotTerminator)))
            )
                .Before(RegionAttributeDivider);

        public static readonly Parser<char, string> UnknownRegionAttribute =
            MultiLineText(nextLineIdent: Char(' ').Repeat(4).IgnoreResult());

        public static Parser<char, int> SilverAmount =
            Char('$').Then(DecimalNum);

        // Wages: $13.3 (Max: $466).
        public static readonly ParserNode RegionWages =
            Sequence(
                Char('$').Then(
                    TNumber.Then(
                        TDot.Then(TNumber).Optional(),
                        (whole, fraction) => {
                            if (fraction.HasValue) {
                                whole += "." + fraction.Value;
                            }

                            return Float("salary", whole);
                        })
                ),
                String("(Max:")
                    .Between(SkipWhitespaces)
                    .Then(SilverAmount)
                    .Num("max-wages")
            )
            .Before(Any.Until(DotTerminator))
            .Node("wages");


        public static readonly ParserNode ItemCode =
            Token(AtlantisCharset().NoWhitespace().Exclude(Charset.List('[', ']')))
                .AtLeastOnceString()
                .BetweenBrackets()
                .Str("code");

        // orc [ORC] at $42
        // 65 orcs [ORC] at $42
        public static readonly ParserNode Item =
            Sequence(
                TNumber
                    .Before(SkipWhitespaces)
                    .RecoverWith(_ => Return("1"))
                    .Num("amount")
                    .LikeOptional(),
                TText(AtlantisCharset(), stopBefore: Lookahead(Try(ItemCode.IgnoreResult())))
                    .Str("name")
                    .LikeOptional(),
                ItemCode
                    .LikeOptional(),
                String("at")
                    .Between(SkipWhitespaces)
                    .Then(SilverAmount)
                    .Num("price")
                    .Optional()
            )
            .Node("item");

        public static readonly ParserNode RegionAttributes =
            String("------------------------------------------------------------")
                .Before(EndOfLine)
                .Then(
                    Char(' ').Repeat(2).Then(
                        OneOf(
                            Try(RegionAttribute.Bind(label => {
                                switch (label) {
                                    case "Wages": return RegionWages;
                                    case "Wanted": return ListOf(Item).Before(DotTerminator).Node("wanted");
                                    case "For Sale": return ListOf(Item).Before(DotTerminator).Node("for-sale");
                                    case "Entertainment available": return SilverAmount.Before(DotTerminator).Num("entertainment-available");
                                    case "Products": return ListOf(Item).Before(DotTerminator).Node("products");

                                    default:
                                        string key = label.ToLowerInvariant().Replace(' ', '-');
                                        return UnknownRegionAttribute.Str(key);
                                }
                            })),
                            UnknownRegionAttribute.Str("unknown")
                        )
                    )
                    .Many()
                )
                .Before(EndOfLine)
                .Node("attributes");

        // n
        // ne
        // ...
        public static readonly ParserNode Direction =
            OneOf(
                Try(String("Northeast").Or(String("NE")).WithResult("ne")),
                Try(String("Southeast").Or(String("SE")).WithResult("se")),
                Try(String("Southwest").Or(String("SW")).WithResult("sw")),
                Try(String("Northwest").Or(String("NW")).WithResult("nw")),
                Try(String("South").Or(String("S")).WithResult("s")),
                Try(String("North").Or(String("N")).WithResult("n"))
            )
            .Str("direction");

        //   Southeast : underforest (51,1,2 <underworld>) in Ryway.
        public static readonly ParserNode Exit =
            String("  ")
                .Then(
                    Sequence(
                        Direction
                            .LikeOptional(),
                        SkipWhitespaces
                            .Then(TColon)
                            .Then(SkipWhitespaces)
                            .Then(Location)
                            .LikeOptional(),
                        TCommaWp
                            .Then(Settlement)
                            .Optional()
                    )
                    .Before(TDot.Then(EndOfLine))
                )
                .Node("exit");
/*
Exits:
  Southeast : underforest (51,1,2 <underworld>) in Ryway.
  South : underforest (50,2,2 <underworld>) in Ryway.
  Southwest : underforest (49,1,2 <underworld>) in Hawheci.
*/
        public static readonly ParserNode Exits =
            String("Exits:")
                .Then(EndOfLine)
                .Then(
                    OneOf(
                        Try(String("  none").WithResult(Enumerable.Empty<IReportNode>())),
                        Exit.Many()
                    )
                )
                .Before(EndOfLine)
                .Node("exits");

        public static ParserNode Unit(string prefix) =>
            Map(
                (owner, line) => {
                    return Str(owner == "*" ? "own-unit" : "unit", line);
                },
                String(prefix + "-").Or(String(prefix + "*"))
                    .Before(Whitespace),
                Sequence(
                    TText(AtlantisCharsetWithParenthesis, stopBefore: Try(EndOfLine).IgnoreResult()),
                    Try(
                        String(prefix + "  ")
                            .Then(
                                TText(AtlantisCharsetWithParenthesis, stopBefore: Try(EndOfLine).IgnoreResult())
                            )
                            .Select(s => " " + s)
                    )
                    .Many()
                    .Select(string.Concat)
                )
                .Select(string.Concat)
            );

        public static ParserNode Units(string prefix) =>
            Unit(prefix)
                .Many()
                .Before(EndOfLine)
                .Node("units");

        // [121]
        public static readonly ParserNode StructureNumber =
            TNumber.BetweenBrackets()
                .Num("number");

        // Fleet
        public static readonly ParserNode StructureType =
            TText(AtlantisCharset().Exclude(Charset.List(',', ';')))
                .Str("type");

        // [121] : Fleet
        public static readonly Parser<char, IEnumerable<IReportNode>> StructureNumberAndType =
            StructureNumber
                .Before(TColon.Between(SkipWhitespaces))
                .Then(
                    StructureType,
                    (num, type) => new[] { num, type } as IEnumerable<IReportNode>
                );

        public static readonly ParserNode StructurePart =
            Sequence(
                DecimalNum.Num("amount"),
                StructureType.Between(SkipWhitespaces)
            )
            .Node("part");

        // 1 Longship, 1 Cog
        public static readonly ParserNode StructureParts =
            ListOf(StructurePart, TCommaWp.IgnoreResult())
                .Node("parts");

        // Load: 580/600
        public static readonly ParserNode StructureAttribute =
            TText(AtlantisCharset().Exclude(Charset.List(':')))
                .Before(TColon.Between(SkipWhitespaces))
                .Then(
                    TText(AtlantisCharset().Exclude(Charset.List(',', ';', '.'))),
                    (key, value) => Node("attribute",
                        Str("key", key),
                        Str("value", value)
                    )
                );

        // Load: 580/600; Sailors: 10/10; MaxSpeed: 4
        public static readonly ParserNode StructureAttributes =
            ListOf(StructureAttribute, TSemicolonWp.IgnoreResult())
                .Node("attributes");

        // needs 10
        public static readonly ParserNode StructureNeeds =
            String("needs")
                .Then(SkipWhitespaces)
                .Then(Int(10))
                .Num("needs");

        // contains an inner location
        public static readonly ParserNode StrucutreFlag =
            TText(AtlantisCharset().Exclude(Charset.List(',', ';', '.')))
                .Str("flag");

        // Fleet
        public static readonly ParserNode StructureName =
            TText(AtlantisCharset(), stopBefore: Lookahead(Try(StructureNumberAndType)).IgnoreResult())
                .Str("name");

        // + Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors: 10/10; MaxSpeed: 4.
        // + 1 [1] : Fort, needs 10.
        // + Shaft [1] : Shaft, contains an inner location.
        // + Building [3] : Fort.
        public static readonly ParserNode Structure =
            String("+ ")
                .Then(
                    Map(
                        (name, numAndType, parts, attributes) => {
                            var n = Node("structure");
                            n.Add(name);
                            n.AddRange(numAndType);

                            if (parts.HasValue) {
                                n.AddRange(parts.Value);
                            }

                            if (attributes.HasValue) {
                                n.Add(attributes.Value);
                            }

                            return n;
                        },
                        StructureName,
                        StructureNumberAndType,
                        TCommaWp
                            .Then(
                                OneOf(
                                    Try(StructureNeeds),
                                    Try(StructureParts),
                                    Try(StrucutreFlag)
                                )
                            )
                            .AtLeastOnce()
                            .Optional(),
                        TSemicolonWp
                            .Then(StructureAttributes)
                            .Optional()
                    )
                )
                .Before(DotTerminator);

        public static readonly ParserNode StructureWithUnits =
            Structure
                .Then(
                    Try(Units("  ").Optional()),
                    (structure, units) => {
                        if (units.HasValue) {
                            structure.Add(units.Value);
                        }

                        return structure;
                    }
                );

        public static readonly ParserNode Structures =
            StructureWithUnits
                .AtLeastOnce()
                .Node("structures");


/*
underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town],
  10237 peasants (drow elves), $5937.
------------------------------------------------------------
  The weather was clear last month; it will be clear next month.
  Wages: $12.9 (Max: $1187).
  Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123
    fish [FISH] at $27, 7 leather armor [LARM] at $69.
  For Sale: 409 drow elves [DRLF] at $41, 81 leaders [LEAD] at $722.
  Entertainment available: $399.
  Products: 17 livestock [LIVE], 12 wood [WOOD], 15 stone [STON], 12
    iron [IRON].

Exits:
  Southeast : underforest (51,1,2 <underworld>) in Ryway.
  South : underforest (50,2,2 <underworld>) in Ryway.
  Southwest : underforest (49,1,2 <underworld>) in Hawheci.
*/
        public static readonly ParserNode Region =
            Sequence(
                Summary
                    .LikeOptional()
                    ,
                RegionAttributes
                    .LikeOptional()
                    ,
                SkipEmptyLines
                    .Then(Exits)
                    .LikeOptional()
                    ,
                SkipEmptyLines
                    .Then(Units(""))
                    .Optional()
                    ,
                SkipEmptyLines
                    .Then(Structures)
                    .Optional()
            )
            .Node("region");

        public static IEnumerable<T> MakeSequence<T>(T first, Maybe<IEnumerable<T>> next) {
            yield return first;

            if (next.HasValue) {
                foreach (var item in next.Value) {
                    yield return item;
                }
            }
        }

        public static IEnumerable<T> MakeSequence<T>(T first, IEnumerable<T> next) {
            yield return first;

            foreach (var item in next) {
                yield return item;
            }
        }

        public static readonly Parser<char, IEnumerable<IReportNode>> RegionSequence =
            Region
                .Then(
                    Rec(() => Try(SkipEmptyLines.Then(RegionSequence))).Optional(),
                    MakeSequence
                );

        public static readonly ParserNode Regions =
            RegionSequence
                .Node("regions");

        public static readonly ParserNode ReportLine =
            OneOf(
                Try(EndOfLine.ThenReturn("")),
                MultiLineText(null)
            )
            .Str("unknown");

        public static readonly ParserNode OrdersTemplate =
            SkipWhitespaces
                .Then(String("Orders Template (Long Format):"))
                .Then(EndOfLine)
                .Then(Any.AtLeastOnce().Before(End))
                .Select(string.Concat)
                .Str("orders-template");

        public static readonly ParserNode Report =
            SkipEmptyLines
                .Then(ReportHeader)
                .Then(SkipEmptyLines
                    .Then(OneOf(
                        Try(ReportFaction),
                        Try(ReportDate),
                        Try(Attitudes),
                        Try(Regions),
                        Try(OrdersTemplate),
                        ReportLine
                    )
                    .Many()
                )
            )
            .Node("report");
    }
}
