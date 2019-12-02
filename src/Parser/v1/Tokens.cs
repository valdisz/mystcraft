namespace atlantis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Pidgin;
    using static Pidgin.Parser;
    using static Pidgin.Parser<char>;

    public static class Charset {
        public static Func<char, bool> List(params char[] chars) {
            return c => {
                return Array.IndexOf(chars, c) >= 0;
            };
        }

        public static Func<char, bool> Range(char from, char to) {
            return c => {
                return c >= from && c <= to;
            };
        }

        public static Func<char, bool> Exclude(this Func<char, bool> charset, Func<char, bool> excluded) {
            return c => {
                return !excluded(c) && charset(c);
            };
        }

        public static Func<char, bool> Combine(params Func<char, bool>[] charsets) {
            return c => {
                for (var i = 0; i < charsets.Length; i++) {
                    if (charsets[i](c)) return true;
                }

                return false;
            };
        }

        public static Func<char, bool> CombineWith(this Func<char, bool> charset, Func<char, bool> other) {
            return c => charset(c) || other(c);
        }

        public static Func<char, bool> ExcludeList(this Func<char, bool> charset, params char[] chars) =>
            charset.Exclude(Charset.List(chars));
    }

    public static class Tokens {
        public static readonly Parser<char, char> TQuote = Char('"');
        public static readonly Parser<char, char> TColon = Char(':');
        public static readonly Parser<char, IEnumerable<char>> TColonWp = TColon.Then(Whitespaces);
        public static readonly Parser<char, char> TSemicolon = Char(';');
        public static readonly Parser<char, IEnumerable<char>> TSemicolonWp = TSemicolon.Then(Whitespaces);
        public static readonly Parser<char, char> TComma = Char(',');
        public static readonly Parser<char, IEnumerable<char>> TCommaWp = TComma.Then(Whitespaces);
        public static readonly Parser<char, char> TDot = Char('.');
        public static readonly Parser<char, char> TDash = Char('-');
        public static readonly Parser<char, IEnumerable<char>> TDashWp = TDash.Then(Whitespaces);
        public static readonly Parser<char, char> TLParenthesis = Char('(');
        public static readonly Parser<char, char> TRParenthesis = Char(')');
        public static readonly Parser<char, char> TLBracket = Char('[');
        public static readonly Parser<char, char> TRBracket = Char(']');

        public static readonly Parser<char, string> TNumber = Digit.AtLeastOnceString();
        public static readonly Parser<char, string> TWord = LetterOrDigit.AtLeastOnceString();

        public static Parser<char, T> BetweenParenthesis<T>(this Parser<char, T> parser) =>
            parser.Between(TLParenthesis, TRParenthesis);

        public static Parser<char, T> BetweenBrackets<T>(this Parser<char, T> parser) =>
            parser.Between(TLBracket, TRBracket);

        public static Parser<char, T> BetweenWhitespaces<T>(this Parser<char, T> parser) =>
            parser.Between(SkipWhitespaces);

        public static Parser<char, T> BeforeWhitespaces<T>(this Parser<char, T> parser) =>
            parser.Before(SkipWhitespaces);


        public static Parser<char, Maybe<T>> LikeOptional<T>(this Parser<char, T> parser) =>
            parser.Select(x => new Maybe<T>(x));

        public static Parser<char, Pidgin.Unit> SkipEmptyLines =
            EndOfLine.SkipMany();

        public static readonly Func<char, bool> AtlantisCharsetWp = Charset.Combine(
            Charset.Range('a', 'z'),
            Charset.Range('A', 'Z'),
            Charset.Range('0', '9'),
            Charset.List(
                ' ', '!', '[', ']', ',', '.', '{', '}', '@', '#', '$',
                '%', '^', '&', '*', '-', '_', '+', '=', ';', ':',
                '<', '>', '?', '/', '~',  '`',
                '\'', '\\'
            )
        );

        public static readonly Func<char, bool> AtlantisCharset = Charset.Combine(
            Charset.Range('a', 'z'),
            Charset.Range('A', 'Z'),
            Charset.Range('0', '9'),
            Charset.List(
                '!', '[', ']', ',', '.', '{', '}', '@', '#', '$',
                '%', '^', '&', '*', '-', '_', '+', '=', ';', ':',
                '<', '>', '?', '/', '~',  '`',
                '\'', '\\'
            )
        );

        public static readonly Func<char, bool> NoSpecials = Charset.Combine(
            Charset.Range('a', 'z'),
            Charset.Range('A', 'Z'),
            Charset.Range('0', '9'),
            Charset.List(
                '\'', '"', '$'
            )
        );

        public static Parser<char, string> TText(Func<char, bool> charset,
            Parser<char, char> space = null,
            Parser<char, Pidgin.Unit> firstLineIdent = null,
            Parser<char, Pidgin.Unit> lineIdent = null) =>
            TText<Pidgin.Unit>(charset, space, firstLineIdent, lineIdent, null);

        public static readonly char[] SingleSpace = { ' ' };

        public static IEnumerable<char> CombineOutputs(char first, Maybe<IEnumerable<char>> next) {
            yield return first;

            if (next.Success) {
                foreach (var c in next.Value) yield return c;
            }
        }

        public static IEnumerable<char> CombineOutputs(IEnumerable<char> first, Maybe<IEnumerable<char>> next) {
            return next.Success
                ? first.Concat(next.Value)
                : first;
        }

        public static Parser<char, string> TText<T>(
            Func<char, bool> charset,
            Parser<char, char> space = null,
            Parser<char, Pidgin.Unit> firstLineIdent = null,
            Parser<char, Pidgin.Unit> lineIdent = null,
            Parser<char, T> terminator = null) {

            if (terminator != null) {
                terminator = Lookahead(Try(terminator));
            }

            var token = terminator == null
                ? Token(charset).AtLeastOnce()
                : Token(charset).AtLeastOnceUntil(terminator);

            space = space ?? Char(' ');
            var spaces = space.AtLeastOnce();

            lineIdent = lineIdent ?? Char(' ').Repeat(2).IgnoreResult();

            Parser<char, IEnumerable<char>> tokens = null;
            // tokens = token.Then(
            //     Rec(() => {
            //         var possible = OneOf(
            //             // next word on the same line
            //             Try(spaces.Then(tokens, (value, next) => value.Concat(next))),

            //             // next word on new line
            //             EndOfLine
            //                 .Then(lineIdent)
            //                 .Then(spaces.Optional())
            //                 .IgnoreResult()
            //                 .Then(
            //                     tokens,
            //                     (value, next) => SingleSpace.Concat(next)
            //                 )
            //         );

            //         return Try(possible);
            //     }).Optional(),
            //     CombineOutputs
            // );

            if (terminator != null) {
                tokens = terminator.ThenReturn(Enumerable.Empty<char>()).Or(tokens);
            }

            return (firstLineIdent != null
                ? firstLineIdent.Then(tokens)
                : tokens
            ).Select(string.Concat);
        }
    }
}
