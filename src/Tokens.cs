namespace atlantis
{
    using System;
    using System.Collections.Generic;
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

        public static Parser<char, Maybe<T>> LikeOptional<T>(this Parser<char, T> parser) =>
            parser.Select(x => new Maybe<T>(x));

        public static Parser<char, Pidgin.Unit> SkipEmptyLines =
            EndOfLine.SkipMany();

        // public static Parser<char, string> SentenceOf(Parser<char, string> word, Parser<char, char> whitespace) {
        //     return Sequence(
        //         word,
        //         Try(
        //             Sequence(whitespace.AtLeastOnceString(), word).Select(string.Concat)
        //         ).ManyString()
        //     ).Select(string.Concat);
        // }

        // public static readonly Parser<char, string> TSentence = SentenceOf(TWord, Whitespace);

        public static Func<char, bool> AtlantisCharset(bool simpleWhitepsace = false) => Charset.Combine(
            c => simpleWhitepsace ? c == ' ' : char.IsWhiteSpace(c),
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

        public static Func<char, bool> NoWhitespace(this Func<char, bool> charser) {
            return charser.Exclude(c => char.IsWhiteSpace(c));
        }

        public static readonly Func<char, bool> AtlantisCharsetWithParenthesis = AtlantisCharset().CombineWith(Charset.List('(', ')'));

        public static Parser<char, string> TText(Func<char, bool> charset, Func<char, bool> separator = null) =>
            TText<Pidgin.Unit>(charset, separator, null);

        public static Parser<char, string> TText<T>(Func<char, bool> charset, Func<char, bool> separator = null, Parser<char, T> stopBefore = null) {
            separator = separator ?? (c => char.IsWhiteSpace(c));

            var firstChar = Token(charset.Exclude(separator));
            var charsetToken = Token(charset.CombineWith(separator));

            Parser<char, string> remaining;
            if (stopBefore == null) {
                remaining = charsetToken.ManyString();
            }
            else {
                remaining = charsetToken.Until(stopBefore).Select(string.Concat);
            }

            return firstChar.Then(remaining, (c, s) => c + s);
        }
    }
}
