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

            return (nodes[0] as ValueReportNode<string>)?.Value;
        }

        public static int? IntValueOf(this IReportNode node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return (nodes[0] as ValueReportNode<int>)?.Value;
        }

        public static double? RealValueOf(this IReportNode node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return (nodes[0] as ValueReportNode<double>)?.Value;
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

        public static IReportNode Node(string type, IEnumerable<Maybe<IReportNode>> children) {
            return new ReportNode(
                type,
                (children ?? Enumerable.Empty<Maybe<IReportNode>>())
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToArray()
            );
        }

        public static IReportNode Str(string type, string value) {
            return new ValueReportNode<string>(type, value.Trim() );
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

        public static IReportNode Real(string type, string value) {
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
            return parser.Select(x => Real(type, x));
        }

        public static ParserNode Float(this Parser<char, double> parser, string type) {
            return parser.Select(x => Float(type, x));
        }
    }

    public static class AtlantisParser {
        public static readonly ParserNone DotTerminator =
            TDot.Then(EndOfLine).IgnoreResult();

        public static readonly Maybe<IReportNode> NoResult = new Maybe<IReportNode>();
        public static readonly IEnumerable<IReportNode> EmptySequence = Enumerable.Empty<IReportNode>();

        public static readonly ParserNone ReportHeader =
            String("Atlantis Report For:").IgnoreResult();

        // War 1
        public static readonly ParserNode FactionAttribute =
            Token(NoSpecials)
                .AtLeastOnceString()
                .Separated(SkipWhitespaces)
                .Where(s => s.Count() > 1)
                .Select(s => Node("attribute",
                    Str("key", string.Join(" ", s.SkipLast(1))),
                    Num("value", s.Last()))
                );

        public static ParserNone UnitTerminator(ParserNone prefix = null) {
            var parser = Sequence(
                TDot,
                Char('\n').Or(Char('\r').Then(Char('\n')))
            );

            var finalPparser = prefix == null
                ? parser.Then(Lookahead(Not(Char(' '))).Optional())
                : parser.Then(Lookahead(Not(prefix.Then(Char(' ')))).Optional());

            return finalPparser.IgnoreResult();
        }

        // (15)
        public static readonly ParserNode FactionNumber =
            Int(10).BetweenParenthesis().Num("number");

        // Avalon Empire
        public static readonly ParserNode FactionName =
            TText(AtlantisCharset)
                .Str("name");

        public static readonly ParserNode Faction =
            Sequence(
                FactionName,
                SkipWhitespaces
                    .Then(FactionNumber)
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
                Faction,
                SkipWhitespaces
                    .Then(FactionAttributes)
            )
            .Node("faction-info");

        // May, Year 3
        public static readonly ParserNode ReportDate =
            Sequence(
                TText(AtlantisCharset.ExcludeList(','))
                    .Before(TCommaWp)
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


        public static Parser<char, IEnumerable<IReportNode>> ListOf(
            ParserNode itemParser,
            Parser<char, Pidgin.Unit> separator = null,
            Parser<char, Pidgin.Unit> terminator = null) {

            separator = separator ?? TCommaWp.IgnoreResult();

            if (terminator != null) {
                terminator = Lookahead(Try(terminator));
            }

            Parser<char, IEnumerable<IReportNode>> recItemParser = null;
            recItemParser = terminator != null
                ? itemParser
                    .Then(
                        terminator.WithResult(EmptySequence)
                            .Or(Rec(() => separator.Then(recItemParser))),
                        MakeSequence
                    )
                : itemParser
                    .Then(
                        Rec(() => separator.Then(recItemParser)).Optional(),
                        MakeSequence
                    );

            return OneOf(
                Try(String("none").WithResult(EmptySequence)),
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
                    .Then(TText(AtlantisCharset.ExcludeList('<', '>')).Between(Char('<'), Char('>')))
                    .Str("label")
                    .Optional()
            )
            .BetweenParenthesis()
            .Node("coords");

        // underforest (0,0,2 <underworld>) in Ryway
        public static readonly ParserNode Location =
            Sequence(
                TText(NoSpecials)
                    .Str("terrain"),
                SkipWhitespaces.Then(Coords),
                SkipWhitespaces
                    .Then(String("in"))
                    .Then(SkipWhitespaces)
                    .Then(TText(AtlantisCharset.ExcludeList(',', '.')))
                    .Str("prvonice")
            )
            .Node("location");

        // [town]
        public static readonly ParserNode SettlementSize =
            TText(AtlantisCharset.ExcludeList('[', ']'))
                .BetweenBrackets()
                .Str("size");

        // contains Sinsto [town]
        public static readonly ParserNode Settlement =
            String("contains")
                .Then(SkipWhitespaces)
                .Then(
                    Sequence(
                        TText(NoSpecials).Str("name"),
                        SkipWhitespaces.Then(SettlementSize)
                    )
                )
                .Node("settlement");

        // 1195 peasants (gnomes)
        public static readonly ParserNode Population =
            Sequence(
                Int(10).Num("amount"),
                String("peasants")
                    .Between(SkipWhitespaces)
                    .Then(TText(NoSpecials).BetweenParenthesis())
                    .Str("race")
            )
            .Node("population");

        public static readonly ParserNode Tax =
            Char('$').Then(Int(10)).Num("tax");

        // underforest (0,0,2 <underworld>) in Ryway, 1195 peasants (gnomes), $621.
        // underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town], 10237 peasants (drow elves), $5937.
        public static readonly ParserNode RegionHeader =
            Location
                .Then(
                    TCommaWp
                        .Then(
                            OneOf(
                                Try(Settlement),
                                Try(Population),
                                Try(Tax)
                            )
                        )
                        .Many()
                        .Optional(),
                    (loc, other) => {
                        var node = Node("region-header", loc);
                        node.AddRange(other.GetValueOrDefault(EmptySequence));
                        return node;
                    }
                )
                .Before(DotTerminator);

        // The weather was clear last month; it will be clear next month.
        public static readonly ParserNone Weather =
            Whitespace.Repeat(2)
                .Then(String("The weather"))
                .Then(TText(AtlantisCharset))
                .IgnoreResult();

        public static readonly Parser<char, string> RegionAttribute =
            TText(NoSpecials)
                .Before(TColon.BeforeWhitespaces());

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

                            return Real("salary", whole);
                        })
                ),
                String("(Max:")
                    .Between(SkipWhitespaces)
                    .Then(SilverAmount)
                    .Num("max-wages")
            )
            .Before(Any.Until(DotTerminator))
            .Node("wages");


        public static readonly ParserNode AtlantisCode =
            Token(NoSpecials)
                .AtLeastOnceString()
                .BetweenBrackets()
                .Str("code");

        public static readonly ParserNode UnitCode = AtlantisCode;

        // orc [ORC] at $42
        // 65 orcs [ORC] at $42
        public static readonly ParserNode Item =
            Sequence(
                TNumber
                    .BeforeWhitespaces()
                    .RecoverWith(_ => Return("1"))
                    .Num("amount"),
                TText(NoSpecials)
                    .Str("name"),
                SkipWhitespaces
                    .Then(UnitCode)
            )
            .Then(
                OneOf(
                    Try(String("at")
                        .BetweenWhitespaces()
                        .Then(SilverAmount)
                        .Num("price")
                    ),
                    Try(
                        SkipWhitespaces
                            .Then(String("needs")
                                .BeforeWhitespaces()
                                .Then(DecimalNum)
                                .BetweenParenthesis()
                            ).Num("needs")
                    )
                ).Many(),
                (itemParams, args) => {
                    var node = Node("item", itemParams);
                    node.AddRange(args);

                    return node;
                }
            );

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
                        Try(String("  none").WithResult(EmptySequence)),
                        Exit.Many()
                    )
                )
                .Before(EndOfLine)
                .Node("exits");

        public static ParserNode UnitName =
            TText(AtlantisCharset)
                .Str("name");

        public static ParserNode UnitNumber =
            DecimalNum
                .BetweenParenthesis()
                .Num("number");

        public static ParserNode UnitFlag =
            TText(NoSpecials)
                .Str("flag");

        public static ParserNode UnitFlags =
            ListOf(
                UnitFlag,
                TCommaWp.IgnoreResult(),
                TCommaWp.Then(Item).IgnoreResult()
            )
            .Node("flags");

        public static ParserNode UnitItems =
            ListOf(
                Item,
                TCommaWp.IgnoreResult(),
                TDot.IgnoreResult().Or(TSemicolonWp.IgnoreResult())
            )
            .Node("items");

        public static ParserNode SkillCode = AtlantisCode;

        public static ParserNode SkillLevel =
            DecimalNum
                .Num("level");

        public static ParserNode SkillDays =
            DecimalNum
                .BetweenParenthesis()
                .Num("days");

        public static Parser<char, IEnumerable<IReportNode>> SkillParameters =
            SkillCode
                .Then(
                    Try(Sequence(
                        SkipWhitespaces
                            .Then(SkillLevel),
                        SkipWhitespaces
                            .Then(SkillDays)
                    )).Optional(),
                    MakeSequence
                );

        public static ParserNode SkillName =
            TText(NoSpecials)
                .Str("name");

        //stealth [STEA] 1 (30)
        public static ParserNode Skill =
            SkillName
                .Before(SkipWhitespaces)
                .Then(
                    SkillParameters,
                    (name, parameters) => {
                        var node = Node("skill");
                        node.Add(name);
                        node.AddRange(parameters);

                        return node;
                    }
                );

        public static Parser<char, int> UnitWight =
            DecimalNum;

        public static Parser<char, IEnumerable<IReportNode>> UnitCapacity =
            Sequence(
                DecimalNum.Num("flying"),
                Char('/')
                    .Then(DecimalNum)
                    .Num("riding"),
                Char('/')
                    .Then(DecimalNum)
                    .Num("walking"),
                Char('/')
                    .Then(DecimalNum)
                    .Num("swimming")
            );

        // . Weight:
        public static Parser<char, string> UnitAttributeName =
            TDot
                .Then(SkipWhitespaces)
                .Then(TText(NoSpecials))
                .Before(TColon.Between(SkipWhitespaces));

        public static Parser<char, string> GenericUnitAttribute =
            TText(AtlantisCharset.ExcludeList(',', '.', ';'));

        public static ParserNode UnitAttribute =
            UnitAttributeName
                .Bind(name => {
                    var key = name.ToLowerInvariant().Replace(' ', '-');
                    var attr = Node("attribute", Str("key", key));

                    switch (name) {
                        case "Weight": return UnitWight.Select(weigth => {
                            attr.Add(Num("value", weigth));
                            return attr;
                        });

                        case "Capacity": return UnitCapacity.Select(cap => {
                            attr.Add(Node("value", cap));
                            return attr;
                        });

                        case "Skills": return ListOf(Skill, terminator: TDot.Or(TSemicolon).IgnoreResult()).Select(skills => {
                            attr.Add(Node("value", skills));
                            return attr;
                        });

                        case "Can Study": return ListOf(Skill, terminator: TDot.Or(TSemicolon).IgnoreResult()).Select(skills => {
                            attr.Add(Node("value", skills));
                            return attr;
                        });

                        default:
                            return GenericUnitAttribute.Select(value => {
                                attr.Add(Str("value", value));
                                return attr;
                            });
                    }
                });

        public static Parser<char, IEnumerable<IReportNode>> UnitAttributeSeq(ParserNone prefix) =>
            UnitAttribute
                .Then(
                    Rec(() => Try(
                        OneOf(
                            Lookahead(Try(TSemicolon)).Then(Return(EmptySequence)),
                            Lookahead(Try(UnitTerminator(prefix))).Then(Return(EmptySequence)),
                            UnitAttributeSeq(prefix)
                        )
                    )).Optional(),
                    MakeSequence
                );

        public static ParserNode UnitAttributes(ParserNone prefix = null) =>
            UnitAttributeSeq(prefix)
                .Node("attributes");

        public static Parser<char, string> UnitDescriptionText(ParserNone prefix) {
            var token = Token(AtlantisCharset.ExcludeList('.')).AtLeastOnce();
            var spaces = Char(' ').AtLeastOnce();
            var lineIdent = Char(' ').Repeat(2).IgnoreResult();

            Parser<char, IEnumerable<char>> tokens = null;
            var recTokens = Rec(() => {
                var nextWord = spaces.Then(tokens, (value, next) => value.Concat(next));
                var newLine = EndOfLine
                    .Then(lineIdent)
                    .Then(spaces.Optional())
                    .IgnoreResult()
                    .Then(
                        tokens,
                        (value, next) => SingleSpace.Concat(next)
                    );
                var terminator = Lookahead(UnitTerminator(prefix)) // .<new line><not 2*space>
                    .ThenReturn(Enumerable.Empty<char>());

                Parser<char, IEnumerable<char>> nextChar = null;
                nextChar = Rec(() => Char('.').Then(
                    OneOf(Try(nextWord), Try(terminator), Try(nextChar), newLine),
                    MakeSequence
                ));

                var possible = OneOf(
                    // next word on the same line
                    Try(nextWord),

                    // end of description
                    Try(terminator),

                    // just . char
                    Try(nextChar),

                    // next word on new line
                    newLine
                );

                return Try(possible);
            }).Optional();

            tokens = token.Then(recTokens, CombineOutputs);

            return tokens.Select(string.Concat);
        }

        public static ParserNode UnitDescription(ParserNone prefix) =>
            UnitDescriptionText(prefix).Str("description");

        public static readonly Parser<char, (IReportNode flags, IReportNode items)> UnitFlagsAndItems =
            Try(UnitFlags.Before(TCommaWp))
                .Optional()
                .Then(
                    UnitItems,
                    (flags, items) => ((flags.GetValueOrDefault((IReportNode) null), items))
                );

        public static Parser<char, Maybe<IReportNode>> UnitFaction =
            // try to check if it is flags and items
            Lookahead(
                Try(
                    TCommaWp
                        .Then(UnitFlagsAndItems)
                        // if yes, then faction is hidden
                        .Then(Return(NoResult))
                )
            )
            // otherwise we can parse faction
            .RecoverWith(_ => TCommaWp.Then(Faction).LikeOptional());

/*
* Unit m2 (2530), Avalon Empire (15), avoiding, behind, revealing
  faction, holding, receiving no aid, won't cross water, wood elf
  [WELF], horse [HORS]. Weight: 60. Capacity: 0/70/85/0. Skills:
  combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).

- Aquatic Scout (3427), Disasters Inc (43), avoiding, behind,
  revealing faction, lizardman [LIZA]. Weight: 10. Capacity:
  0/0/15/15. Skills: none.

- Scout (2070), Disasters Inc (43), avoiding, behind, revealing
  faction, receiving no aid, high elf [HELF], horse [HORS], 40 silver
  [SILV]. Weight: 60. Capacity: 0/70/85/0. Skills: riding [RIDI] 1
  (60).

- Herdsmen of the Ungre Guild (1767), 3 humans [MAN], 3 spices [SPIC],
  2 gems [GEM], net [NET], 2 mink [MINK], 3 livestock [LIVE]; Content
  looking shepherds and herdsmen.
 */
        public static ParserNode Unit(ParserNone prefix = null) {
            var parser = Map(
                (marker, name, number, faction, flagsAndItems, attributes, description) => {
                    var node = Node(
                        marker == '*'
                            ? "own-unit"
                            : "unit",
                        name,
                        number
                    );

                    if (faction.HasValue) {
                        node.Add(faction.Value);
                    }

                    if (description.HasValue) {
                        node.Add(description.Value);
                    }

                    var (flags, items) = flagsAndItems;

                    if (flags != null) {
                        node.Add(flags);
                    }

                    if (attributes.HasValue) {
                        node.Add(attributes.Value);
                    }

                    node.AddRange(items);

                    return node;
                },
                Char('-').Or(Char('*'))
                    .Before(Whitespace),
                UnitName,
                SkipWhitespaces
                    .Then(UnitNumber),
                UnitFaction,
                TCommaWp
                    .Then(UnitFlagsAndItems),
                Try(UnitAttributes(prefix))
                    .Optional(),
                Try(TSemicolonWp.Then(UnitDescription(prefix)))
                    .Optional()
            ).Before(UnitTerminator(prefix));

            return prefix != null
                ? prefix.Then(parser)
                : parser;
        }

        public static Parser<char, IEnumerable<IReportNode>> UnitSequence(ParserNone prefix = null) =>
            SkipEmptyLines
                .Then(Unit(prefix))
                .Then(
                    Rec(() => Try(UnitSequence(prefix))).Optional(),
                    MakeSequence
                );

        public static ParserNode Units(ParserNone prefix = null) =>
            UnitSequence(prefix)
                .Node("units");

        // [121]
        public static readonly ParserNode StructureNumber =
            DecimalNum
                .BetweenBrackets()
                .Num("number");

        // Fleet
        public static readonly ParserNode StructureType =
            TText(NoSpecials)
                .Str("type");

        // [121] : Fleet
        public static readonly Parser<char, IEnumerable<IReportNode>> StructureNumberAndType =
            SkipWhitespaces
                .Then(StructureNumber)
                .Before(TColon.BetweenWhitespaces())
                .Then(
                    StructureType,
                    (num, type) => new[] { num, type } as IEnumerable<IReportNode>
                );

        public static readonly ParserNode StructurePart =
            Sequence(
                DecimalNum
                    .Num("amount"),
                StructureType
                    .BetweenWhitespaces()
            )
            .Node("part");

        // 1 Longship, 1 Cog
        public static readonly ParserNode StructureParts =
            ListOf(StructurePart, TCommaWp.IgnoreResult())
                .Node("parts");

        // ; Load: 580/600
        public static readonly ParserNode StructureAttribute =
            TSemicolonWp
                .Then(TText(NoSpecials))
                .Before(TColon.BetweenWhitespaces())
                .Then(
                    TText(NoSpecials.CombineWith(Charset.List('/', ','))),
                    (key, value) => Node("attribute",
                        Str("key", key),
                        Str("value", value)
                    )
                );

        public static readonly Parser<char, IEnumerable<IReportNode>> StructureAttributeSeq =
            StructureAttribute
                .Then(
                    Rec(() => Try(StructureAttributeSeq)).Optional(),
                    MakeSequence
                );

        // ; Load: 580/600; Sailors: 10/10; MaxSpeed: 4
        public static readonly ParserNode StructureAttributes =
            StructureAttributeSeq
                .Node("attributes");

        // needs 10
        public static readonly ParserNode StructureNeeds =
            String("needs")
                .Then(SkipWhitespaces)
                .Then(DecimalNum)
                .Num("needs");

        // contains an inner location
        public static readonly ParserNode StrucutreFlag =
            TText(NoSpecials)
                .Str("flag");

        // Fleet
        // AE Explorer
        public static Parser<char, string> StructureNameText() {
            var token = Token(AtlantisCharset).AtLeastOnce();
            var spaces = Char(' ').AtLeastOnce();
            var lineIdent = Char(' ').Repeat(2).IgnoreResult();

            Parser<char, IEnumerable<char>> tokens = null;
            var recTokens = Rec(() => {
                var nextWord = spaces.Then(tokens, (value, next) => value.Concat(next));
                var newLine = EndOfLine
                    .Then(lineIdent)
                    .Then(spaces.Optional())
                    .IgnoreResult()
                    .Then(
                        tokens,
                        (value, next) => SingleSpace.Concat(next)
                    );
                var terminator = Lookahead(SkipWhitespaces.Then(StructureNumberAndType)) // [123] : Fleet
                    .ThenReturn(Enumerable.Empty<char>());

                var possible = OneOf(
                    // end of description
                    Try(terminator),

                    // next word on the same line
                    Try(nextWord),

                    // just . char
                    Try(Char('.').Then(
                        OneOf(Try(nextWord), Try(terminator), newLine),
                        MakeSequence
                    )),

                    // next word on new line
                    newLine
                );

                return Try(possible);
            }).Optional();

            tokens = token.Then(recTokens, CombineOutputs);

            return tokens.Select(string.Concat);
        }

        public static readonly ParserNode StructureName =
            StructureNameText()
                .Str("name");

        public static Parser<char, string> StructureDescriptionText() {
            var token = Token(AtlantisCharset.ExcludeList('.')).AtLeastOnce();
            var spaces = Char(' ').AtLeastOnce();
            var lineIdent = Char(' ').Repeat(2).IgnoreResult();

            Parser<char, IEnumerable<char>> tokens = null;
            var recTokens = Rec(() => {
                var nextWord = spaces.Then(tokens, (value, next) => value.Concat(next));
                var newLine = EndOfLine
                    .Then(lineIdent)
                    .Then(spaces.Optional())
                    .IgnoreResult()
                    .Then(
                        tokens,
                        (value, next) => SingleSpace.Concat(next)
                    );
                var terminator = Lookahead(DotTerminator) // .<new line><not 2*space>
                    .ThenReturn(Enumerable.Empty<char>());

                var possible = OneOf(
                    // next word on the same line
                    Try(nextWord),

                    // end of description
                    Try(terminator),

                    // just . char
                    Try(Char('.').Then(
                        OneOf(Try(nextWord), Try(terminator), newLine),
                        MakeSequence
                    )),

                    // next word on new line
                    newLine
                );

                return Try(possible);
            }).Optional();

            tokens = token.Then(recTokens, CombineOutputs);

            return tokens.Select(string.Concat);
        }

        public static readonly ParserNode StructureDescription =
            TSemicolonWp
                .Then(StructureDescriptionText())
                .Str("description");

        // + Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors: 10/10; MaxSpeed: 4.
        // + 1 [1] : Fort, needs 10.
        // + Shaft [1] : Shaft, contains an inner location.
        // + Building [3] : Fort.
        // + AE Sembury [165] : Cog; Load: 500/500; Sailors: 6/6; MaxSpeed: 4; Imperial Trade Fleet.
        public static readonly ParserNode Structure =
            String("+ ")
                .Then(
                    Map(
                        (name, numAndType, parts, attributes, description) => {
                            var n = Node("structure");
                            n.Add(name);
                            n.AddRange(numAndType);

                            if (description.HasValue) {
                                n.Add(description.Value);
                            }

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
                        Try(StructureAttributes)
                            .Optional(),
                        Try(StructureDescription)
                            .Optional()
                    )
                )
                .Before(DotTerminator);

        public static readonly ParserNode StructureWithUnits =
            Structure
                .Then(
                    Try(
                        Units(Char(' ').Repeat(2).IgnoreResult())

                    ).Optional()
                    // .LikeOptional()
                    ,
                    (structure, units) => {
                        if (units.HasValue) {
                            structure.Add(units.Value);
                        }

                        return structure;
                    }
                );

        public static readonly Parser<char, IEnumerable<IReportNode>> StructuresSequence =
            SkipEmptyLines
                .Then(StructureWithUnits)
                .Then(
                    Rec(() => Try(StructuresSequence)).Optional(),
                    MakeSequence
                );

        public static readonly ParserNode Structures =
            StructuresSequence
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
                RegionHeader,
                RegionAttributes,
                SkipEmptyLines
                    .Then(Exits)
            )
            .Then(
                Sequence(
                    Try(Units()).Optional(),
                    Try(Structures).Optional()
                ),
                (required, optional) => {
                    var node = Node("region");

                    node.AddRange(required);
                    node.AddRange(optional
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                    );

                    return node;
                }
            );

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
                .Then(
                    Map(
                        (faction, date, attitudes, regions, orders) => {
                            var node = Node("report",
                                faction,
                                date,
                                attitudes,
                                regions,
                                orders
                            );
                            return node;
                        },
                        SkipEmptyLines.Then(ReportFaction),
                        SkipEmptyLines.Then(ReportDate),
                        SkipEmptyLines
                            .Then(ReportLine
                                .SkipUntil(Lookahead(Try(Attitudes)))
                            )
                            .Then(Attitudes),
                        SkipEmptyLines
                            .Then(ReportLine
                                .SkipUntil(Lookahead(Try(Region)))
                            )
                            .Then(Regions),
                        SkipEmptyLines.Then(OrdersTemplate)
                    )
                );
    }
}
