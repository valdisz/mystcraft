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

    enum ReportSection {
        General,
        Errors,
        Battles,
        Events,
        SkillLore,
        ItemLore,
        ObjectLore,
        Region,
        OrderTemplate,
        Other
    }

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
    }

    public static class RegionParser {
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
                .Str("faction-name");

        // (15)
        public static readonly ParserNode FactionNumber =
            Int(10).BetweenParenthesis().Num("faction-number");

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
        public static ParserNode Attitude =
            OneOf(
                Try(String("Hostile")),
                Try(String("Unfriendly")),
                Try(String("Neutral")),
                Try(String("Friendly")),
                String("Ally")
            )
            .Str("attitude");

        // Declared Attitudes (default Neutral):
        public static ParserNode DefaultAttitude =
            String("Declared Attitudes (default ")
                .Then(Attitude)
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
                        Attitude
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

        public static readonly Parser<char, string> RegionAttributeLine =
            String("  ")
                .Then(
                    Any
                        .Until(EndOfLine)
                        .Select(string.Concat)
                );

        public static readonly Parser<char, IEnumerable<string>> RegionAttribute =
            RegionAttributeLine
                .Then(
                    Rec(() => Try(String("  ").Then(RegionAttribute))).Optional(),
                    MakeSequence
                );

        public static readonly ParserNode RegionAttributes =
            String("------------------------------------------------------------")
                .Before(EndOfLine)
                .Then(
                    RegionAttribute
                        .Select(s => string.Join(" ", s))
                        .Str("attr")
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
            Sequence(
                String(prefix + "-")
                    .Or(String(prefix + "*"))
                    .Then(Whitespace)
                    .Then(
                        TText(AtlantisCharsetWithParenthesis, stopBefore: Try(EndOfLine).IgnoreResult())
                    ),
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
            .Str("unit");

        public static ParserNode Units(string prefix) =>
            Unit(prefix)
                .Many()
                .Before(EndOfLine)
                .Node("units");

        public static readonly ParserNode Structure =
            Sequence(
                String("+ ")
                    .Then(
                        TText(AtlantisCharset(), stopBefore: EndOfLine.IgnoreResult())
                    )
                    .Str("structure-info")
                    .LikeOptional(),
                Units("  ")
                    .Optional()
            )
            .Node("structure");

        public static readonly ParserNode Structures =
            Structure
                .Many()
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

        public static readonly Parser<char, IEnumerable<string>> ReportLineSequence =
            TText(c => !char.IsWhiteSpace(c), stopBefore: EndOfLine.IgnoreResult())
                .Then(
                    Rec(() => Char(' ')
                        .AtLeastOnce()
                        .Then(ReportLineSequence, (_, line) => MakeSequence(" ", line))
                    ).Optional(),
                    MakeSequence
                );

        public static readonly ParserNode ReportLine =
            OneOf(
                Try(EndOfLine.ThenReturn("")),
                ReportLineSequence.Select(string.Concat)
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
            // SkipEmptyLines
            //     .Then(GeneralParsers.ReportHeader)
            //     .Then(
            //         SkipEmptyLines
            //             .Then(
            //                 OneOf(
            //                     Try(GeneralParsers.FactionInfo),
            //                     Try(GeneralParsers.Date),
            //                     Try(Regions),
            //                     Try(OrdersTemplate),
            //                     Try(ReportLine)
            //                 )
            //             )
            //             .Many()
            //     )
                // .Node("report");
    }
}
