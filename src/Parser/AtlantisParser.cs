// namespace atlantis
// {
//     using System.Collections.Generic;
//     using System.Linq;
//     using Pidgin;
//     using static Pidgin.Parser;
//     using static Pidgin.Parser<char>;
//     using static Tokens;
//     using static Nodes;

//     using ParserNode = Pidgin.Parser<char, IReportNodeOld>;
//     using ParserNone = Pidgin.Parser<char, Pidgin.Unit>;
//     using System;
//     using System.Text;

//     public static class AtlantisParser2 {
//         public static readonly Parser<char, string> AText =
//             Token(AtlantisCharsetWp)
//                 .AtLeastOnceString();

//         public static readonly Parser<char, string> AEngineText =
//             Token(AtlantisCharsetWp.ExcludeList('[', ']'))
//                 .AtLeastOnceString();

//         public static readonly Parser<char, string> ACode =
//             Token(Charset.Combine(
//                 Charset.Range('A', 'Z'),
//                 Charset.Range('0', '9')
//             ))
//                 .AtLeastOnceString();

//         public static readonly ParserNode FullSkill =
//             Sequence(
//                 AEngineText.Str("name"),
//                 ACode.BetweenBrackets().Str("code"),
//                 Int(10).BetweenWhitespaces().Num("level"),
//                 Int(10).BetweenParenthesis().Num("days")
//             ).Node("skill", false, false);

//         public static readonly ParserNode BaseSkill =
//             Sequence(
//                 AEngineText.Str("name"),
//                 ACode.BetweenBrackets().Str("code")
//             ).Node("skill", false, false);

//         public static readonly ParserNode UnitOrFactionName =
//             Sequence(
//                 AText.Str("name"),
//                 Int(10).BetweenParenthesis().Num("")
//             )
//             .Node("unitOrFactionName", false, false);
//     }

//     public static class AtlantisParser {
//         public static readonly ParserNone DotTerminator =
//             TDot.Then(EndOfLine).IgnoreResult();

//         public static readonly Maybe<IReportNodeOld> NoResult = new Maybe<IReportNodeOld>();
//         public static readonly IEnumerable<IReportNodeOld> EmptySequence = Enumerable.Empty<IReportNodeOld>();

//         public static readonly ParserNone ReportHeader =
//             String("Atlantis Report For:").IgnoreResult();

//         // War 1
//         public static readonly ParserNode FactionAttribute =
//             Token(NoSpecials)
//                 .AtLeastOnceString()
//                 .Separated(SkipWhitespaces)
//                 .Where(s => s.Count() > 1)
//                 .Select(s => Node("attribute", asArray: false, writeProperty: false,
//                     Str("key", string.Join(" ", s.SkipLast(1))),
//                     Num("value", s.Last()))
//                 );

//         public static ParserNone UnitTerminator(ParserNone prefix = null) {
//             var parser = Sequence(
//                 TDot,
//                 Char('\n').Or(Char('\r').Then(Char('\n')))
//             );

//             var finalPparser = prefix == null
//                 ? parser.Then(Lookahead(Not(Char(' '))).Optional())
//                 : parser.Then(Lookahead(Not(prefix.Then(Char(' ')))).Optional());

//             return finalPparser.IgnoreResult();
//         }

//         // (15)
//         public static readonly ParserNode FactionNumber =
//             Int(10).BetweenParenthesis().Num("number");

//         // Avalon Empire
//         public static readonly ParserNode FactionName =
//             TText(AtlantisCharset)
//                 .Str("name");

//         public static readonly ParserNode Faction =
//             Sequence(
//                 FactionName,
//                 SkipWhitespaces
//                     .Then(FactionNumber)
//             )
//             .Node("faction", asArray: false, writeProperty: true);

//         // (War 1, Trade 2, Magic 2)
//         public static readonly ParserNode FactionAttributes =
//             FactionAttribute
//                 .Separated(TCommaWp)
//                 .BetweenParenthesis()
//                 .Node("attributes", asArray: true, writeProperty: true);

//         // Avalon Empire (15) (War 1, Trade 2, Magic 2)
//         public static readonly ParserNode ReportFaction =
//             Sequence(
//                 Faction,
//                 SkipWhitespaces
//                     .Then(FactionAttributes)
//             )
//             .Node("faction-info", asArray: false, writeProperty: true);

//         // May, Year 3
//         public static readonly ParserNode ReportDate =
//             Sequence(
//                 TText(AtlantisCharset.ExcludeList(','))
//                     .Before(TCommaWp)
//                     .Str("month"),
//                 String("Year")
//                     .Then(SkipWhitespaces)
//                     .Then(Int(10))
//                     .Num("year")
//             )
//             .Node("date", asArray: false, writeProperty: true);

// /*
// Declared Attitudes (default Unfriendly):
// Hostile : none.
// Unfriendly : Creatures (2).
// Neutral : none.
// Friendly : Semigallians (18), Disasters Inc (43).
// Ally : none.
// */
//         public static ParserNode Stance =
//             OneOf(
//                 Try(String("Hostile").Select(x => x.ToLowerInvariant())),
//                 Try(String("Unfriendly").Select(x => x.ToLowerInvariant())),
//                 Try(String("Neutral").Select(x => x.ToLowerInvariant())),
//                 Try(String("Friendly").Select(x => x.ToLowerInvariant())),
//                 String("Ally").Select(x => x.ToLowerInvariant())
//             )
//             .Str("stance");

//         // Declared Attitudes (default Neutral):
//         public static ParserNode DefaultAttitude =
//             String("Declared Attitudes (default ")
//                 .Then(Stance)
//                 .Before(String("):"));


//         public static Parser<char, IEnumerable<IReportNodeOld>> ListOf(
//             ParserNode itemParser,
//             Parser<char, Pidgin.Unit> separator = null,
//             Parser<char, Pidgin.Unit> terminator = null) {

//             separator = separator ?? TCommaWp.IgnoreResult();

//             if (terminator != null) {
//                 terminator = Lookahead(Try(terminator));
//             }

//             Parser<char, IEnumerable<IReportNodeOld>> recItemParser = null;
//             recItemParser = terminator != null
//                 ? itemParser
//                     .Then(
//                         terminator.WithResult(EmptySequence)
//                             .Or(Rec(() => separator.Then(recItemParser))),
//                         MakeSequence
//                     )
//                 : itemParser
//                     .Then(
//                         Rec(() => separator.Then(recItemParser)).Optional(),
//                         MakeSequence
//                     );

//             return OneOf(
//                 Try(String("none").WithResult(EmptySequence)),
//                 recItemParser
//             );
//         }

//         public static ParserNode ListOfFactions =
//             ListOf(Faction)
//                 .Before(TDot.Then(EndOfLine))
//                 .Node("factions", asArray: true, writeProperty: true);

//         public static ParserNode Attitudes =
//             DefaultAttitude
//                 .Before(EndOfLine)
//                 .Then(
//                     Sequence(
//                         Stance
//                             .Before(TColon.Between(SkipWhitespaces)),
//                         ListOfFactions
//                     )
//                     .Node("attitude", asArray: false, writeProperty: false)
//                     .Repeat(5),
//                     (def, list) => {
//                         var node = Node("attitudes", asArray: false, writeProperty: true);

//                         node.Add(Str("default", (def as ValueReportNode<string>).Value));
//                         node.AddRange(list.Select(x => Node(
//                             x.StrValueOf("stance"), asArray: true, true,
//                             x.FirstByType("factions").Children
//                                 .Select(f => Node("faction", asArray: false, writeProperty: false, f.Children.ToArray()))
//                         )));

//                         return node;
//                     }
//                 );

//         // (0,0,2 <underworld>)
//         public static readonly ParserNode Coords =
//             Sequence(
//                 Int(10)
//                     .Num("x")
//                     .LikeOptional(),
//                 TComma
//                     .Then(Int(10))
//                     .Num("y")
//                     .LikeOptional(),
//                 TComma
//                     .Then(Int(10))
//                     .Num("z")
//                     .Optional(),
//                 SkipWhitespaces
//                     .Then(Token(AtlantisCharset.ExcludeList('<', '>')).AtLeastOnceString().Between(Char('<'), Char('>')))
//                     .Str("label")
//                     .Optional()
//             )
//             .BetweenParenthesis()
//             .Node("coords", asArray: false, writeProperty: true);

//         // underforest (0,0,2 <underworld>) in Ryway
//         public static readonly ParserNode Location =
//             Sequence(
//                 TText(NoSpecials)
//                     .Str("terrain"),
//                 SkipWhitespaces.Then(Coords),
//                 SkipWhitespaces
//                     .Then(String("in"))
//                     .Then(SkipWhitespaces)
//                     .Then(TText(AtlantisCharset.ExcludeList(',', '.')))
//                     .Str("province")
//             )
//             .Node("location", asArray: false, writeProperty: true);

//         // [town]
//         public static readonly ParserNode SettlementSize =
//             TText(AtlantisCharset.ExcludeList('[', ']'))
//                 .BetweenBrackets()
//                 .Str("size");

//         // contains Sinsto [town]
//         public static readonly ParserNode Settlement =
//             String("contains")
//                 .Then(SkipWhitespaces)
//                 .Then(
//                     Sequence(
//                         TText(NoSpecials).Str("name"),
//                         SkipWhitespaces.Then(SettlementSize)
//                     )
//                 )
//                 .Node("settlement", asArray: false, writeProperty: true);

//         // 1195 peasants (gnomes)
//         public static readonly ParserNode Population =
//             Sequence(
//                 Int(10).Num("amount"),
//                 String("peasants")
//                     .Between(SkipWhitespaces)
//                     .Then(TText(NoSpecials).BetweenParenthesis())
//                     .Str("race")
//             )
//             .Node("population", asArray: false, writeProperty: true);

//         public static readonly ParserNode Tax =
//             Char('$').Then(Int(10)).Num("tax");

//         // underforest (0,0,2 <underworld>) in Ryway, 1195 peasants (gnomes), $621.
//         // underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town], 10237 peasants (drow elves), $5937.
//         public static readonly ParserNode RegionHeader =
//             Location
//                 .Then(
//                     TCommaWp
//                         .Then(
//                             OneOf(
//                                 Try(Settlement),
//                                 Try(Population),
//                                 Try(Tax)
//                             )
//                         )
//                         .Many()
//                         .Optional(),
//                     (loc, other) => {
//                         var node = Node("region-header", asArray: false, writeProperty: true, loc);
//                         node.AddRange(other.GetValueOrDefault(EmptySequence));
//                         return node;
//                     }
//                 )
//                 .Before(DotTerminator);

//         // The weather was clear last month; it will be clear next month.
//         public static readonly ParserNone Weather =
//             Whitespace.Repeat(2)
//                 .Then(String("The weather"))
//                 .Then(TText(AtlantisCharset))
//                 .IgnoreResult();

//         public static readonly Parser<char, string> RegionAttribute =
//             TText(NoSpecials)
//                 .Before(TColon.BeforeWhitespaces());

//         public static Parser<char, string> MultiLineText(
//             Parser<char, Pidgin.Unit> prefix = null,
//             Parser<char, Pidgin.Unit> nextLineIdent = null,
//             Parser<char, Pidgin.Unit> firstLineMarker = null) {

//             var line = Any.Until(EndOfLine).Select(string.Concat);
//             nextLineIdent = nextLineIdent ?? Char(' ').Repeat(2).IgnoreResult();

//             if (prefix != null) {
//                 line = prefix.Then(line);
//                 nextLineIdent = prefix.Then(nextLineIdent);
//             }

//             Parser<char, IEnumerable<string>> recLine = null;
//             recLine = line
//                 .Then(
//                     Rec(() => Try(nextLineIdent.Then(recLine)).Optional()),
//                     MakeSequence
//                 );

//             var parser = recLine.Select(s => string.Join(" ", s));

//             return firstLineMarker != null
//                 ? firstLineMarker.Then(parser)
//                 : parser;
//         }

//         public static readonly Parser<char, string> UnknownRegionAttribute =
//             MultiLineText(nextLineIdent: Char(' ').Repeat(4).IgnoreResult());

//         public static Parser<char, int> SilverAmount =
//             Char('$').Then(DecimalNum);

//         // Wages: $13.3.
//         // Wages: $13.3 (Max: $466).
//         public static readonly ParserNode RegionWages =
//             Sequence(
//                 Char('$').Then(
//                     TNumber.Then(
//                         TDot.Then(TNumber).Optional(),
//                         (whole, fraction) => {
//                             if (fraction.HasValue) {
//                                 whole += "." + fraction.Value;
//                             }

//                             return Float("salary", whole);
//                         })
//                 ).LikeOptional(),
//                 Try(String("(Max:")
//                     .Between(SkipWhitespaces)
//                     .Then(SilverAmount)
//                     .Num("max-wages")
//                 ).Optional()
//             )
//             .Before(Any.Until(DotTerminator))
//             .Node("wages", asArray: false, writeProperty: true);


//         public static readonly ParserNode AtlantisCode =
//             Token(NoSpecials)
//                 .AtLeastOnceString()
//                 .BetweenBrackets()
//                 .Str("code");

//         public static readonly ParserNode UnitCode = AtlantisCode;

//         // orc [ORC] at $42
//         // 65 orcs [ORC] at $42
//         public static readonly ParserNode Item =
//             Sequence(
//                 TNumber
//                     .BeforeWhitespaces()
//                     .RecoverWith(_ => Return("1"))
//                     .Num("amount"),
//                 TText(NoSpecials)
//                     .Str("name"),
//                 SkipWhitespaces
//                     .Then(UnitCode)
//             )
//             .Then(
//                 OneOf(
//                     Try(String("at")
//                         .BetweenWhitespaces()
//                         .Then(SilverAmount)
//                         .Num("price")
//                     ),
//                     Try(
//                         SkipWhitespaces
//                             .Then(String("needs")
//                                 .BeforeWhitespaces()
//                                 .Then(DecimalNum)
//                                 .BetweenParenthesis()
//                             ).Num("needs")
//                     )
//                 ).Many(),
//                 (itemParams, args) => {
//                     var node = Node("item", asArray: false, writeProperty: false, itemParams);
//                     node.AddRange(args);

//                     return node;
//                 }
//             );

//         public static readonly Parser<char, IEnumerable<IReportNodeOld>> RegionAttributes =
//             String("------------------------------------------------------------")
//                 .Before(EndOfLine)
//                 .Then(
//                     Char(' ').Repeat(2).Then(
//                         OneOf(
//                             Try(RegionAttribute.Bind(label => {
//                                 switch (label) {
//                                     case "Wages": return RegionWages;
//                                     case "Wanted": return ListOf(Item).Before(DotTerminator).Node("wanted", asArray: true, writeProperty: true);
//                                     case "For Sale": return ListOf(Item).Before(DotTerminator).Node("forSale", asArray: true, writeProperty: true);
//                                     case "Entertainment available": return SilverAmount.Before(DotTerminator).Num("entertainmentAvailable");
//                                     case "Products": return ListOf(Item).Before(DotTerminator).Node("products", asArray: true, writeProperty: true);

//                                     default:
//                                         string key = label.ToLowerInvariant().Replace(' ', '-');
//                                         return UnknownRegionAttribute
//                                             .Select(s => Node("unknown", asArray: false, writeProperty: false,
//                                                 Str("key", key),
//                                                 Str("value", s)
//                                             ));
//                                 }
//                             })),
//                             UnknownRegionAttribute
//                                 .Select(s => Node("unknown", asArray: false, writeProperty: false,
//                                     Str("key", "unknown"),
//                                     Str("value", s)
//                                 ))
//                         )
//                     )
//                     .AtLeastOnce()
//                 )
//                 .Before(EndOfLine);

//         // n
//         // ne
//         // ...
//         public static readonly ParserNode Direction =
//             OneOf(
//                 Try(String("Northeast").Or(String("NE")).WithResult("ne")),
//                 Try(String("Southeast").Or(String("SE")).WithResult("se")),
//                 Try(String("Southwest").Or(String("SW")).WithResult("sw")),
//                 Try(String("Northwest").Or(String("NW")).WithResult("nw")),
//                 Try(String("South").Or(String("S")).WithResult("s")),
//                 Try(String("North").Or(String("N")).WithResult("n"))
//             )
//             .Str("direction");

//         //   Southeast : underforest (51,1,2 <underworld>) in Ryway.
//         public static readonly ParserNode Exit =
//             String("  ")
//                 .Then(
//                     Sequence(
//                         Direction
//                             .LikeOptional(),
//                         SkipWhitespaces
//                             .Then(TColon)
//                             .Then(SkipWhitespaces)
//                             .Then(Location)
//                             .LikeOptional(),
//                         TCommaWp
//                             .Then(Settlement)
//                             .Optional()
//                     )
//                     .Before(TDot.Then(EndOfLine))
//                 )
//                 .Node("exit", asArray: false, writeProperty: false);
// /*
// Exits:
//   Southeast : underforest (51,1,2 <underworld>) in Ryway.
//   South : underforest (50,2,2 <underworld>) in Ryway.
//   Southwest : underforest (49,1,2 <underworld>) in Hawheci.
// */
//         public static readonly ParserNode Exits =
//             String("Exits:")
//                 .Then(EndOfLine)
//                 .Then(
//                     OneOf(
//                         Try(String("  none").WithResult(EmptySequence)),
//                         Exit.Many()
//                     )
//                 )
//                 .Before(EndOfLine)
//                 .Node("exits", asArray: true, writeProperty: true);

//         public static ParserNode UnitName =
//             TText(AtlantisCharset)
//                 .Str("name");

//         public static ParserNode UnitNumber =
//             DecimalNum
//                 .BetweenParenthesis()
//                 .Num("number");

//         public static ParserNode UnitFlag =
//             TText(NoSpecials)
//                 .Str("flag", writeProperty: false);

//         public static ParserNode UnitFlags =
//             ListOf(
//                 UnitFlag,
//                 TCommaWp.IgnoreResult(),
//                 TCommaWp.Then(Item).IgnoreResult()
//             )
//             .Node("flags", asArray: true, writeProperty: true);

//         public static ParserNode UnitItems =
//             ListOf(
//                 Item,
//                 TCommaWp.IgnoreResult(),
//                 TDot.IgnoreResult().Or(TSemicolonWp.IgnoreResult())
//             )
//             .Node("items", asArray: true, writeProperty: true);

//         public static ParserNode SkillCode = AtlantisCode;

//         public static ParserNode SkillLevel =
//             DecimalNum
//                 .Num("level");

//         public static ParserNode SkillDays =
//             DecimalNum
//                 .BetweenParenthesis()
//                 .Num("days");

//         public static Parser<char, IEnumerable<IReportNodeOld>> SkillParameters =
//             SkillCode
//                 .Then(
//                     Try(Sequence(
//                         SkipWhitespaces
//                             .Then(SkillLevel),
//                         SkipWhitespaces
//                             .Then(SkillDays)
//                     )).Optional(),
//                     MakeSequence
//                 );

//         public static ParserNode SkillName =
//             TText(NoSpecials)
//                 .Str("name");

//         //stealth [STEA] 1 (30)
//         public static ParserNode Skill =
//             SkillName
//                 .Before(SkipWhitespaces)
//                 .Then(
//                     SkillParameters,
//                     (name, parameters) => {
//                         var node = Node("skill", asArray: false, writeProperty: false);
//                         node.Add(name);
//                         node.AddRange(parameters);

//                         return node;
//                     }
//                 );

//         public static Parser<char, int> UnitWight =
//             DecimalNum;

//         public static Parser<char, IEnumerable<IReportNodeOld>> UnitCapacity =
//             Sequence(
//                 DecimalNum.Num("flying"),
//                 Char('/')
//                     .Then(DecimalNum)
//                     .Num("riding"),
//                 Char('/')
//                     .Then(DecimalNum)
//                     .Num("walking"),
//                 Char('/')
//                     .Then(DecimalNum)
//                     .Num("swimming")
//             );

//         // . Weight:
//         public static Parser<char, string> UnitAttributeName =
//             TDot
//                 .Then(SkipWhitespaces)
//                 .Then(TText(NoSpecials))
//                 .Before(TColon.Between(SkipWhitespaces));

//         public static Parser<char, string> GenericUnitAttribute =
//             TText(AtlantisCharset.ExcludeList(',', '.', ';'));

//         public static ParserNode UnitAttribute =
//             UnitAttributeName
//                 .Bind(name => {

//                     switch (name) {
//                         case "Weight": return UnitWight.Num("weight");
//                         case "Capacity": return UnitCapacity.Node("capacity", asArray: false, writeProperty: true);
//                         case "Skills": return ListOf(Skill, terminator: TDot.Or(TSemicolon).IgnoreResult())
//                             .Node("skills", asArray: true, writeProperty: true);
//                         case "Can Study": return ListOf(Skill, terminator: TDot.Or(TSemicolon).IgnoreResult())
//                             .Node("canStudy", asArray: true, writeProperty: true);

//                         default:
//                             return GenericUnitAttribute.Select(value => {
//                                 var key = name.ToLowerInvariant().Replace(' ', '-');
//                                 var attr = Node("unknown", asArray: false, writeProperty: false,
//                                     Str("key", key),
//                                     Str("value", value)
//                                 );
//                                 return attr;
//                             });
//                     }
//                 });

//         public static Parser<char, IEnumerable<IReportNodeOld>> UnitAttributes(ParserNone prefix = null) =>
//             UnitAttribute
//                 .Then(
//                     Rec(() => Try(
//                         OneOf(
//                             Lookahead(Try(TSemicolon)).Then(Return(EmptySequence)),
//                             Lookahead(Try(UnitTerminator(prefix))).Then(Return(EmptySequence)),
//                             UnitAttributes(prefix)
//                         )
//                     )).Optional(),
//                     MakeSequence
//                 );

//         public static Parser<char, string> UnitDescriptionText(ParserNone prefix) {
//             var token = Token(AtlantisCharset.ExcludeList('.')).AtLeastOnce();
//             var spaces = Char(' ').AtLeastOnce();
//             var lineIdent = Char(' ').Repeat(2).IgnoreResult();

//             Parser<char, IEnumerable<char>> tokens = null;
//             var recTokens = Rec(() => {
//                 var nextWord = spaces.Then(tokens, (value, next) => value.Concat(next));
//                 var newLine = EndOfLine
//                     .Then(lineIdent)
//                     .Then(spaces.Optional())
//                     .IgnoreResult()
//                     .Then(
//                         tokens,
//                         (value, next) => SingleSpace.Concat(next)
//                     );
//                 var terminator = Lookahead(UnitTerminator(prefix)) // .<new line><not 2*space>
//                     .ThenReturn(Enumerable.Empty<char>());

//                 Parser<char, IEnumerable<char>> nextChar = null;
//                 nextChar = Rec(() => Char('.').Then(
//                     OneOf(Try(nextWord), Try(terminator), Try(nextChar), newLine),
//                     MakeSequence
//                 ));

//                 var possible = OneOf(
//                     // next word on the same line
//                     Try(nextWord),

//                     // end of description
//                     Try(terminator),

//                     // just . char
//                     Try(nextChar),

//                     // next word on new line
//                     newLine
//                 );

//                 return Try(possible);
//             }).Optional();

//             tokens = token.Then(recTokens, CombineOutputs);

//             return tokens.Select(string.Concat);
//         }

//         public static ParserNode UnitDescription(ParserNone prefix) =>
//             UnitDescriptionText(prefix).Str("description");

//         public static readonly Parser<char, (IReportNodeOld flags, IReportNodeOld items)> UnitFlagsAndItems =
//             Try(UnitFlags.Before(TCommaWp))
//                 .Optional()
//                 .Then(
//                     UnitItems,
//                     (flags, items) => ((flags.GetValueOrDefault((IReportNodeOld) null), items))
//                 );

//         public static Parser<char, Maybe<IReportNodeOld>> UnitFaction =
//             // try to check if it is flags and items
//             Lookahead(
//                 Try(
//                     TCommaWp
//                         .Then(UnitFlagsAndItems)
//                         // if yes, then faction is hidden
//                         .Then(Return(NoResult))
//                 )
//             )
//             // otherwise we can parse faction
//             .RecoverWith(_ => TCommaWp.Then(Faction).LikeOptional());

// /*
// * Unit m2 (2530), Avalon Empire (15), avoiding, behind, revealing
//   faction, holding, receiving no aid, won't cross water, wood elf
//   [WELF], horse [HORS]. Weight: 60. Capacity: 0/70/85/0. Skills:
//   combat [COMB] 1 (30), stealth [STEA] 1 (30), riding [RIDI] 1 (65).

// - Aquatic Scout (3427), Disasters Inc (43), avoiding, behind,
//   revealing faction, lizardman [LIZA]. Weight: 10. Capacity:
//   0/0/15/15. Skills: none.

// - Scout (2070), Disasters Inc (43), avoiding, behind, revealing
//   faction, receiving no aid, high elf [HELF], horse [HORS], 40 silver
//   [SILV]. Weight: 60. Capacity: 0/70/85/0. Skills: riding [RIDI] 1
//   (60).

// - Herdsmen of the Ungre Guild (1767), 3 humans [MAN], 3 spices [SPIC],
//   2 gems [GEM], net [NET], 2 mink [MINK], 3 livestock [LIVE]; Content
//   looking shepherds and herdsmen.
//  */
//         public static ParserNode Unit(ParserNone prefix = null) {
//             var parser = Map(
//                 (marker, name, number, faction, flagsAndItems, attributes, description) => {
//                     var node = Node(
//                         "unit",
//                         asArray: false, writeProperty: false,
//                         name,
//                         number
//                     );

//                     node.Add(new ValueReportNode<bool>("own", marker == '*'));

//                     if (faction.HasValue) {
//                         node.Add(faction.Value);
//                     }

//                     if (description.HasValue) {
//                         node.Add(description.Value);
//                     }

//                     var (flags, items) = flagsAndItems;

//                     if (flags != null) {
//                         node.Add(flags);
//                     }

//                     if (attributes.HasValue) {
//                         var unknown = Node("attributes", asArray: true, writeProperty: true);
//                         foreach (var a in attributes.Value) {
//                             if (a.Type == "unknown") {
//                                 unknown.Add(a);
//                             }
//                             else {
//                                 node.Add(a);
//                             }
//                         }

//                         if (unknown.Children.Count > 0) {
//                             node.Add(unknown);
//                         }
//                     }

//                     node.AddRange(items);

//                     return node;
//                 },
//                 Char('-').Or(Char('*'))
//                     .Before(Whitespace),
//                 UnitName,
//                 SkipWhitespaces
//                     .Then(UnitNumber),
//                 UnitFaction,
//                 TCommaWp
//                     .Then(UnitFlagsAndItems),
//                 Try(UnitAttributes(prefix))
//                     .Optional(),
//                 Try(TSemicolonWp.Then(UnitDescription(prefix)))
//                     .Optional()
//             ).Before(UnitTerminator(prefix));

//             return prefix != null
//                 ? prefix.Then(parser)
//                 : parser;
//         }

//         public static Parser<char, IEnumerable<IReportNodeOld>> UnitSequence(ParserNone prefix = null) =>
//             SkipEmptyLines
//                 .Then(Unit(prefix))
//                 .Then(
//                     Rec(() => Try(UnitSequence(prefix))).Optional(),
//                     MakeSequence
//                 );

//         public static ParserNode Units(ParserNone prefix = null) =>
//             UnitSequence(prefix)
//                 .Node("units", asArray: true, writeProperty: true);

//         // [121]
//         public static readonly ParserNode StructureNumber =
//             DecimalNum
//                 .BetweenBrackets()
//                 .Num("number");

//         // Fleet
//         public static readonly ParserNode StructureType =
//             TText(NoSpecials)
//                 .Str("type");

//         // [121] : Fleet
//         public static readonly Parser<char, IEnumerable<IReportNodeOld>> StructureNumberAndType =
//             SkipWhitespaces
//                 .Then(StructureNumber)
//                 .Before(TColon.BetweenWhitespaces())
//                 .Then(
//                     StructureType,
//                     (num, type) => new[] { num, type } as IEnumerable<IReportNodeOld>
//                 );

//         public static readonly ParserNode StructurePart =
//             Sequence(
//                 DecimalNum
//                     .Num("amount"),
//                 StructureType
//                     .BetweenWhitespaces()
//             )
//             .Node("part", asArray: false, writeProperty: false);

//         // 1 Longship, 1 Cog
//         public static readonly ParserNode StructureParts =
//             ListOf(StructurePart, TCommaWp.IgnoreResult())
//                 .Node("parts", asArray: true, writeProperty: true);

//         // ; Load: 580/600
//         public static readonly ParserNode StructureAttribute =
//             TSemicolonWp
//                 .Then(TText(NoSpecials))
//                 .Before(TColon.BetweenWhitespaces())
//                 .Then(
//                     TText(NoSpecials.CombineWith(Charset.List('/', ','))),
//                     (key, value) => Node("attribute",
//                         asArray: false, writeProperty: false,
//                         Str("key", key),
//                         Str("value", value)
//                     )
//                 );

//         public static readonly Parser<char, IEnumerable<IReportNodeOld>> StructureAttributeSeq =
//             StructureAttribute
//                 .Then(
//                     Rec(() => Try(StructureAttributeSeq)).Optional(),
//                     MakeSequence
//                 );

//         // ; Load: 580/600; Sailors: 10/10; MaxSpeed: 4
//         public static readonly ParserNode StructureAttributes =
//             StructureAttributeSeq
//                 .Node("attributes", asArray: true, writeProperty: true);

//         // needs 10
//         public static readonly ParserNode StructureNeeds =
//             String("needs")
//                 .Then(SkipWhitespaces)
//                 .Then(DecimalNum)
//                 .Num("needs");

//         // contains an inner location
//         public static readonly ParserNode StrucutreFlag =
//             TText(NoSpecials)
//                 .Str("flag");

//         // Fleet
//         // AE Explorer
//         public static Parser<char, string> StructureNameText() {
//             var token = Token(AtlantisCharset).AtLeastOnce();
//             var spaces = Char(' ').AtLeastOnce();
//             var lineIdent = Char(' ').Repeat(2).IgnoreResult();

//             Parser<char, IEnumerable<char>> tokens = null;
//             var recTokens = Rec(() => {
//                 var nextWord = spaces.Then(tokens, (value, next) => value.Concat(next));
//                 var newLine = EndOfLine
//                     .Then(lineIdent)
//                     .Then(spaces.Optional())
//                     .IgnoreResult()
//                     .Then(
//                         tokens,
//                         (value, next) => SingleSpace.Concat(next)
//                     );
//                 var terminator = Lookahead(SkipWhitespaces.Then(StructureNumberAndType)) // [123] : Fleet
//                     .ThenReturn(Enumerable.Empty<char>());

//                 var possible = OneOf(
//                     // end of description
//                     Try(terminator),

//                     // next word on the same line
//                     Try(nextWord),

//                     // just . char
//                     Try(Char('.').Then(
//                         OneOf(Try(nextWord), Try(terminator), newLine),
//                         MakeSequence
//                     )),

//                     // next word on new line
//                     newLine
//                 );

//                 return Try(possible);
//             }).Optional();

//             tokens = token.Then(recTokens, CombineOutputs);

//             return tokens.Select(string.Concat);
//         }

//         public static readonly ParserNode StructureName =
//             StructureNameText()
//                 .Str("name");

//         public static Parser<char, string> StructureDescriptionText() {
//             var token = Token(AtlantisCharset.ExcludeList('.')).AtLeastOnce();
//             var spaces = Char(' ').AtLeastOnce();
//             var lineIdent = Char(' ').Repeat(2).IgnoreResult();

//             Parser<char, IEnumerable<char>> tokens = null;
//             var recTokens = Rec(() => {
//                 var nextWord = spaces.Then(tokens, (value, next) => value.Concat(next));
//                 var newLine = EndOfLine
//                     .Then(lineIdent)
//                     .Then(spaces.Optional())
//                     .IgnoreResult()
//                     .Then(
//                         tokens,
//                         (value, next) => SingleSpace.Concat(next)
//                     );
//                 var terminator = Lookahead(DotTerminator) // .<new line><not 2*space>
//                     .ThenReturn(Enumerable.Empty<char>());

//                 Parser<char, IEnumerable<char>> nextChar = null;
//                 nextChar = Rec(() => Char('.').Then(
//                     OneOf(Try(nextWord), Try(terminator), Try(nextChar), newLine),
//                     MakeSequence
//                 ));

//                 var possible = OneOf(
//                     // next word on the same line
//                     Try(nextWord),

//                     // end of description
//                     Try(terminator),

//                     // just . char
//                     Try(nextChar),

//                     // next word on new line
//                     newLine
//                 );

//                 return Try(possible);
//             }).Optional();

//             tokens = token.Then(recTokens, CombineOutputs);

//             return tokens.Select(string.Concat);
//         }

//         public static readonly ParserNode StructureDescription =
//             TSemicolonWp
//                 .Then(StructureDescriptionText())
//                 .Str("description");

//         // + Fleet [121] : Fleet, 1 Longship, 1 Cog; Load: 580/600; Sailors: 10/10; MaxSpeed: 4.
//         // + 1 [1] : Fort, needs 10.
//         // + Shaft [1] : Shaft, contains an inner location.
//         // + Building [3] : Fort.
//         // + AE Sembury [165] : Cog; Load: 500/500; Sailors: 6/6; MaxSpeed: 4; Imperial Trade Fleet.
//         public static readonly ParserNode Structure =
//             String("+ ")
//                 .Then(
//                     Map(
//                         (name, numAndType, parts, attributes, description) => {
//                             var n = Node("structure", asArray: false, writeProperty: false);
//                             n.Add(name);
//                             n.AddRange(numAndType);

//                             if (description.HasValue) {
//                                 n.Add(description.Value);
//                             }

//                             if (parts.HasValue) {
//                                 n.AddRange(parts.Value);
//                             }

//                             if (attributes.HasValue) {
//                                 n.Add(attributes.Value);
//                             }

//                             return n;
//                         },
//                         StructureName,
//                         StructureNumberAndType,
//                         TCommaWp
//                             .Then(
//                                 OneOf(
//                                     Try(StructureNeeds),
//                                     Try(StructureParts),
//                                     Try(StrucutreFlag)
//                                 )
//                             )
//                             .AtLeastOnce()
//                             .Optional(),
//                         Try(StructureAttributes)
//                             .Optional(),
//                         Try(StructureDescription)
//                             .Optional()
//                     )
//                 )
//                 .Before(DotTerminator);

//         public static readonly ParserNode StructureWithUnits =
//             Structure
//                 .Then(
//                     Try(
//                         Units(Char(' ').Repeat(2).IgnoreResult())

//                     ).Optional()
//                     // .LikeOptional()
//                     ,
//                     (structure, units) => {
//                         if (units.HasValue) {
//                             structure.Add(units.Value);
//                         }

//                         return structure;
//                     }
//                 );

//         public static readonly Parser<char, IEnumerable<IReportNodeOld>> StructuresSequence =
//             SkipEmptyLines
//                 .Then(StructureWithUnits)
//                 .Then(
//                     Rec(() => Try(StructuresSequence)).Optional(),
//                     MakeSequence
//                 );

//         public static readonly ParserNode Structures =
//             StructuresSequence
//                 .Node("structures", asArray: true, writeProperty: true);

// /*
// underforest (50,0,2 <underworld>) in Ryway, contains Sinsto [town],
//   10237 peasants (drow elves), $5937.
// ------------------------------------------------------------
//   The weather was clear last month; it will be clear next month.
//   Wages: $12.9 (Max: $1187).
//   Wanted: 167 grain [GRAI] at $20, 115 livestock [LIVE] at $20, 123
//     fish [FISH] at $27, 7 leather armor [LARM] at $69.
//   For Sale: 409 drow elves [DRLF] at $41, 81 leaders [LEAD] at $722.
//   Entertainment available: $399.
//   Products: 17 livestock [LIVE], 12 wood [WOOD], 15 stone [STON], 12
//     iron [IRON].

// Exits:
//   Southeast : underforest (51,1,2 <underworld>) in Ryway.
//   South : underforest (50,2,2 <underworld>) in Ryway.
//   Southwest : underforest (49,1,2 <underworld>) in Hawheci.
// */
//         public static readonly ParserNode Region =
//             Map(
//                 (header, attributes, exits, optional) => {
//                     var node = Node("region", asArray: false, writeProperty: false);
//                     node.Add(header);

//                     var unknown = Node("attributes", asArray: true, writeProperty: true);
//                     foreach (var a in attributes) {
//                         if (a.Type == "unknown") {
//                             unknown.Add(a);
//                         }
//                         else {
//                             node.Add(a);
//                         }
//                     }

//                     if (unknown.Children.Count > 0) {
//                         node.Add(unknown);
//                     }

//                     node.AddRange(optional
//                         .Where(x => x.HasValue)
//                         .Select(x => x.Value)
//                     );

//                     return node;
//                 },
//                 RegionHeader,
//                 RegionAttributes,
//                 SkipEmptyLines
//                     .Then(Exits),
//                 Sequence(
//                     Try(Units()).Optional(),
//                     Try(Structures).Optional()
//                 )
//             );

//         public static IEnumerable<T> MakeSequence<T>(T first, Maybe<IEnumerable<T>> next) {
//             yield return first;

//             if (next.HasValue) {
//                 foreach (var item in next.Value) {
//                     yield return item;
//                 }
//             }
//         }

//         public static IEnumerable<T> MakeSequence<T>(T first, IEnumerable<T> next) {
//             yield return first;

//             foreach (var item in next) {
//                 yield return item;
//             }
//         }

//         public static readonly Parser<char, IEnumerable<IReportNodeOld>> RegionSequence =
//             Region
//                 .Then(
//                     Rec(() => Try(SkipEmptyLines.Then(RegionSequence))).Optional(),
//                     MakeSequence
//                 );

//         public static readonly ParserNode Regions =
//             RegionSequence
//                 .Node("regions", asArray: true, writeProperty: true);

//         public static readonly ParserNode ReportLine =
//             OneOf(
//                 Try(EndOfLine.ThenReturn("")),
//                 MultiLineText(null)
//             )
//             .Str("unknown");

//         public static readonly ParserNode OrdersTemplate =
//             SkipWhitespaces
//                 .Then(String("Orders Template (Long Format):"))
//                 .Then(EndOfLine)
//                 .Then(Any.AtLeastOnce().Before(End))
//                 .Select(string.Concat)
//                 .Str("orders-template");

//         public static readonly ParserNode Report =
//             SkipEmptyLines
//                 .Then(ReportHeader)
//                 .Then(
//                     Map(
//                         (faction, date, attitudes, regions, orders) => {
//                             var node = Node("report",
//                                 asArray: false,
//                                 writeProperty: false,
//                                 faction,
//                                 date,
//                                 attitudes,
//                                 regions,
//                                 orders
//                             );
//                             return node;
//                         },
//                         SkipEmptyLines.Then(ReportFaction),
//                         SkipEmptyLines.Then(ReportDate),
//                         SkipEmptyLines
//                             .Then(ReportLine
//                                 .SkipUntil(Lookahead(Try(Attitudes)))
//                             )
//                             .Then(Attitudes),
//                         SkipEmptyLines
//                             .Then(ReportLine
//                                 .SkipUntil(Lookahead(Try(Region)))
//                             )
//                             .Then(Regions),
//                         SkipEmptyLines.Then(OrdersTemplate)
//                     )
//                 );
//     }
// }
