namespace advisor {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TextParser {
        public TextParser(int ln, string text) {
            Ln = ln;
            Text = text.AsMemory();

        }

        public TextParser(int ln, ReadOnlyMemory<char> text) {
            Ln = ln;
            Text = text;
        }

        private readonly Stack<int> bookmarks = new Stack<int>();
        public int Pos { get; private set; } = 0;

        public ReadOnlyMemory<char> Text { get; }
        public bool EOF => Text.Length == Pos;
        public int Ln { get; }
        public int Length => Text.Length - Pos;

        private ReadOnlySpan<char> GetSpan() => Text.Span.Slice(Pos);

        private char Peek() {
            return Text.Span[Pos];
        }

        private int LFind(ReadOnlySpan<char> s) {
            if (EOF) return -1;

            var span = GetSpan();
            if (span.Length < s.Length) return -1;

            for (var i = 0; i < span.Length; i++) {
                var value = span.Slice(i);
                if (value.StartsWith(s, StringComparison.OrdinalIgnoreCase)) {
                    return i;
                }
            }

            return -1;
        }

        private int RFind(ReadOnlySpan<char> s) {
            if (EOF) return -1;

            var span = GetSpan();
            if (span.Length < s.Length) return -1;

            for (var i = span.Length - s.Length; i >= 0; i--) {
                var value = span.Slice(i);
                if (value.StartsWith(s, StringComparison.OrdinalIgnoreCase)) {
                    return i;
                }
            }

            return -1;
        }

        public TextParser Reset() {
            ClearBookmarks();
            Pos = 0;
            return this;
        }

        public Maybe<TextParser> Seek(int n) {
            var newPos = Pos + n;
            if (newPos < 0 || newPos > Text.Length) {
                return new Maybe<TextParser>("EOF", Ln, Pos + 1);
            }

            Pos = newPos;
            return new Maybe<TextParser>(this);
        }

        public TextParser PushBookmark() {
            bookmarks.Push(Pos);
            return this;
        }

        public TextParser PopBookmark() {
            Pos = bookmarks.Pop();
            return this;
        }

        public TextParser ClearBookmarks() {
            bookmarks.Clear();
            return this;
        }

        public TextParser RemoveBookmark() {
            bookmarks.Pop();
            return this;
        }

        public Maybe<TextParser> Slice(int length) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var s = new TextParser(Ln, Text.Slice(Pos, length));
            Seek(length);
            return new Maybe<TextParser>(s);
        }

        public Maybe<TextParser> Before(ReadOnlySpan<char> s) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var i = LFind(s);
            if (i >= 0) {
                return new Maybe<TextParser>(Slice(i));
            }

            return new Maybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public Maybe<TextParser> Before(params string[] list) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            int i = -1;
            foreach (var item in list) {
                var j = LFind(item);
                if (j < 0) continue;

                if (i < 0 || j < i) {
                    i = j;
                }
            }

            if (i >= 0) {
                return new Maybe<TextParser>(Slice(i));
            }

            return new Maybe<TextParser>($"Cant find any of {string.Join(",", list)}", Ln, Pos + 1);
        }

        public Maybe<TextParser> After(ReadOnlySpan<char> s) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var i = LFind(s);
            if (i >= 0) {
                Seek(i + s.Length);
                return new Maybe<TextParser>(this);
            }

            return new Maybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public Maybe<TextParser> BeforeBackwards(ReadOnlySpan<char> s) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var i = RFind(s);
            if (i >= 0) {
                return new Maybe<TextParser>(Slice(i));
            }

            return new Maybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public Maybe<TextParser> AfterBackwards(ReadOnlySpan<char> s) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var i = RFind(s);
            if (i >= 0) {
                Seek(i + s.Length);
                return new Maybe<TextParser>(this);
            }

            return new Maybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public Maybe<TextParser> SkipWhitespaces(int minTimes = 0) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = 0; i < span.Length; i++) {
                if (!char.IsWhiteSpace(span[i])) {
                    if (i < minTimes) {
                        return new Maybe<TextParser>($"Whitespace must present at least {minTimes} times", Ln, Pos + 1);
                    }

                    break;
                }
            }

            return Seek(i);
        }

        public Maybe<TextParser> SkipWhitespacesBackwards(int minTimes = 0) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = span.Length - 1; i >= 0; i--) {
                if (!char.IsWhiteSpace(span[i])) {
                    if ((span.Length - i - 1) < minTimes) {
                        return new Maybe<TextParser>($"Whitespace must present at least {minTimes} times", Ln, Pos + 1);
                    }

                    break;
                }
            }

            return Slice(i + 1);
        }

        public Maybe<TextParser> SkipChar(char c) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = 0; i < span.Length; i++) {
                if (span[i] != c) {
                    break;
                }
            }

            return Seek(i);
        }

        public Maybe<TextParser> Skip(Func<char, bool> predicate) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = 0; i < span.Length; i++) {
                if (!predicate(span[i])) {
                    break;
                }
            }

            return Seek(i);
        }

        public Maybe<TextParser> Then(ReadOnlySpan<char> s) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            if (span.Length < s.Length || !span.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                return new Maybe<TextParser>("does not match", Ln, Pos + 1);

            return Seek(s.Length);
        }

        public Maybe<int> Integer() {
            if (EOF) return new Maybe<int>("EOF", Ln, Pos + 1);

            PushBookmark();
            var span = GetSpan();

            var i = 0;
            switch (span[0]) {
                case '+':
                    var r = Seek(1);
                    if (!r) return r.Convert<int>();

                    span = GetSpan();
                    break;

                case '-':
                    i++;
                    break;
            }

            if (!char.IsDigit(span[i])) {
                PopBookmark();
                return new Maybe<int>("Not a number", Ln, Pos + 1);
            }

            for (; i < span.Length; i++) {
                if (!char.IsDigit(span[i])) {
                    break;
                }
            }

            if (i < 0) {
                PopBookmark();
                return new Maybe<int>("Expected +, - or number", Ln, Pos + 1);
            }

            RemoveBookmark();

            var s = Slice(i);
            if (!s) return s.Convert<int>();

            return new Maybe<int>(int.Parse(s.Value.Text.Span));
        }

        public Maybe<double> Real() {
            if (EOF) return new Maybe<double>("EOF", Ln, Pos + 1);

            PushBookmark();
            var span = GetSpan();

            var i = 0;
            switch (span[0]) {
                case '+':
                    var r = Seek(1);
                    if (!r) return r.Convert<double>();

                    span = GetSpan();
                    break;

                case '-':
                    i++;
                    break;
            }

            if (!char.IsDigit(span[i]) && span[i] != '.') {
                PopBookmark();
                return new Maybe<double>("Not a number", Ln, Pos + 1);
            }

            for (; i < span.Length; i++) {
                if (!char.IsDigit(span[i]) && span[i] != '.') {
                    break;
                }
            }

            if (i < 0) {
                PopBookmark();
                return new Maybe<double>("Expected +, -, . or number", Ln, Pos + 1);
            }

            RemoveBookmark();

            var s = Slice(i);
            if (!s) return s.Convert<double>();

            return new Maybe<double>(double.Parse(s.Value.Text.Span));
        }

        public Maybe<TextParser> Word() {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            PushBookmark();
            SkipWhitespaces();

            if (EOF) {
                PopBookmark();
                return new Maybe<TextParser>("Reached EOF", Ln, Pos + 1);
            }

            var span = GetSpan();
            int i;
            for (i = 0; i < span.Length; i++) {
                var c = span[i];
                if (char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSeparator(c)) {
                    break;
                }
            }

            if (i == 0) {
                PopBookmark();
                return new Maybe<TextParser>("Not a word", Ln, Pos + 1);
            }

            RemoveBookmark();
            return new Maybe<TextParser>(Slice(i));
        }

        public Maybe<string[]> Words() {
            if (EOF) return new Maybe<string[]>("EOF", Ln, Pos + 1);

            PushBookmark();

            var first = Word();
            if (!first) {
                PopBookmark();
                return first.Convert<string[]>();
            }

            // todo: w.Value.Length == 0 is workardound
            if (first.Value.Length == 0) {
                PopBookmark();
                return new Maybe<string[]>("Not a word", Ln, Pos);
            }

            List<string> words = new List<string> { first.Value };

            while (!EOF) {
                PushBookmark();

                SkipWhitespaces();
                var w = Word();
                if (!w) {
                    PopBookmark();
                    break;
                }

                RemoveBookmark();
                words.Add(w.Value);
            }

            RemoveBookmark();
            return new Maybe<string[]>(words.ToArray());
        }

        public bool StartsWith(ReadOnlySpan<char> s) {
            if (EOF) return false;

            var span = GetSpan();

            return span.StartsWith(s, StringComparison.OrdinalIgnoreCase);
        }

        public bool EndsWith(ReadOnlySpan<char> s) {
            if (EOF) return false;

            var span = GetSpan();

            return span.EndsWith(s, StringComparison.OrdinalIgnoreCase);
        }

        public bool Contains(ReadOnlySpan<char> s, StringComparison? comparison = null) {
            if (EOF) return false;

            var span = GetSpan();

            return span.Contains(s, comparison ?? StringComparison.OrdinalIgnoreCase);
        }

        public Maybe<TextParser> Match(ReadOnlySpan<char> s) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            if (span.Length < s.Length || !span.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                return new Maybe<TextParser>("does not match", Ln, Pos + 1);

            return Slice(s.Length);
        }

        public Maybe<TextParser> MatchEnd(ReadOnlySpan<char> s) {
            if (EOF) return new Maybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            if (span.Length < s.Length || !span.EndsWith(s, StringComparison.OrdinalIgnoreCase))
                return new Maybe<TextParser>("does not match", Ln, Pos + 1);

            return Slice(span.Length - s.Length);
        }

        public Maybe<TextParser> Between(ReadOnlySpan<char> left, ReadOnlySpan<char> right, bool useSkip = false) {
            PushBookmark();

            var result = useSkip ? Then(left) : After(left);
            if (!result) {
                PopBookmark();
                return result;
            }

            result = result.Value.Before(right);
            if (result) {
                Seek(right.Length);
                RemoveBookmark();
            }
            else {
                PopBookmark();
            }

            return result;
        }

        public Maybe<TextParser> Between(ReadOnlySpan<char> s, bool useSkip = false) => Between(s, s, useSkip);

        public override string ToString() => GetSpan().ToString();

        public string AsString() => ToString();

        public static implicit operator string(TextParser p) => p.ToString();
    }

    public static class TextParserExtensions {
        public static Maybe<TextParser> Reset(this Maybe<TextParser> p) => p ? new Maybe<TextParser>(p.Value.Reset()) : p;
        public static Maybe<TextParser> Seek(this Maybe<TextParser> p, int n) => p ? new Maybe<TextParser>(p.Value.Seek(n)) : p;

        public static Maybe<TextParser> Before(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Before(s) : p;
        public static Maybe<TextParser> After(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.After(s) : p;
        public static Maybe<TextParser> BeforeBackwards(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.BeforeBackwards(s) : p;
        public static Maybe<TextParser> AfterBackwards(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.AfterBackwards(s) : p;

        public static Maybe<TextParser> Before(this Maybe<TextParser> p, params string[] list) => p ? p.Value.Before(list) : p;

        public static Maybe<TextParser> After(this TextParser p, params string[] list) {
            foreach (var s in list) {
                var match = p.After(s);
                if (match) return match;
            }

            return new Maybe<TextParser>("all options does not fit", p.Ln, p.Pos + 1);
        }

        public static Maybe<TextParser> SkipWhitespaces(this Maybe<TextParser> p, int minTimes = 0) {
            if (!p) return p;
            return p.Value.SkipWhitespaces(minTimes);
        }

        public static Maybe<TextParser> SkipWhitespacesBackwards(this Maybe<TextParser> p, int minTimes = 0) {
            if (!p) return p;
            return p.Value.SkipWhitespacesBackwards(minTimes);
        }

        public static Maybe<TextParser> Word(this Maybe<TextParser> p) => p ? p.Value.Word() : p;
        public static Maybe<TextParser> Match(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Match(s) : p;
        public static Maybe<TextParser> MatchEnd(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.MatchEnd(s) : p;
        public static bool StartsWith(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.StartsWith(s) : false;
        public static Maybe<int> Integer(this Maybe<TextParser> p) => p ? p.Value.Integer() : p.Convert<int>();
        public static Maybe<double> Real(this Maybe<TextParser> p) => p ? p.Value.Real() : p.Convert<double>();
        public static Maybe<TextParser> Between(this Maybe<TextParser> p, ReadOnlySpan<char> left, ReadOnlySpan<char> right, bool useThen = false) => p ? p.Value.Between(left, right, useThen) : p;
        public static Maybe<TextParser> Between(this Maybe<TextParser> p, ReadOnlySpan<char> s, bool useThen = false) => p ? p.Value.Between(s, useThen) : p;
        public static Maybe<string> AsString(this Maybe<TextParser> p) => p ? new Maybe<string>(p.Value) : p.Convert<string>();

        public static Maybe<T> Try<T>(this Maybe<TextParser> p, Func<TextParser, Maybe<T>> parser)
            => p ? Try(p.Value, parser) : p.Convert<T>();

        public static Maybe<IReportNode> Try(this TextParser parser, IReportParser reportParser)
            => Try(parser, reportParser.Parse);

        public static Maybe<IReportNode> Try(this Maybe<TextParser> parser, IReportParser reportParser)
            => Try(parser, reportParser.Parse);

        public static Maybe<T> Try<T>(this TextParser p, Func<TextParser, Maybe<T>> parser) {
            p.PushBookmark();
            Maybe<T> result = parser(p);

            if (!result) {
                p.PopBookmark();
            }
            else {
                p.RemoveBookmark();
            }

            return result;
        }

        public static Maybe<T> OneOf<T>(this Maybe<TextParser> p, params Func<TextParser, Maybe<T>>[] parsers)
            => p ? OneOf(p.Value, parsers) : p.Convert<T>();

        public static Maybe<T> OneOf<T>(this TextParser p, params Func<TextParser, Maybe<T>>[] parsers) {
            for (var i = 0; i < parsers.Length; i++) {
                var result = p.Try(parsers[i]);
                if (result) return result;
            }

            return new Maybe<T>("all options does not fit", p.Ln, p.Pos + 1);
        }

        public static Maybe<IReportNode> OneOf(this Maybe<TextParser> p, params IReportParser[] parsers)
            => p ? OneOf(p.Value, parsers) : p.Convert<IReportNode>();

        public static Maybe<TextParser> OneOf(this TextParser p, params string[] str) {
            for (var i = 0; i < str.Length; i++) {
                var result = p.Try(parser => parser.Match(str[i]));
                if (result) return result;
            }

            return new Maybe<TextParser>("all options does not fit", p.Ln, p.Pos + 1);
        }

        public static Maybe<TextParser> OneOf(this Maybe<TextParser> p, params string[] str)
            => p ? OneOf(p.Value, str) : p.Convert<TextParser>();

        public static Maybe<IReportNode> OneOf(this TextParser p, params IReportParser[] parsers) {
            for (var i = 0; i < parsers.Length; i++) {
                var result = p.Try(parsers[i]);
                if (result) return result;
            }

            return new Maybe<IReportNode>("all options does not fit", p.Ln, p.Pos + 1);
        }

        public static Maybe<T[]> List<T>(this TextParser p, ReadOnlySpan<char> separator, Func<TextParser, Maybe<T>> itemParser) {
            List<T> items = new List<T>();

            if (p.Match("none")) {
                return new Maybe<T[]>(new T[0]);
            }

            while (!p.EOF) {
                Maybe<TextParser> span = p.Before(separator);
                if (span) p.Seek(separator.Length);

                var item = itemParser(span ? span : p);
                if (!item) return item.Convert<T[]>();

                items.Add(item.Value);
                if (!span) {
                    break;
                }
            }

            return new Maybe<T[]>(items.ToArray());
        }

        public static Maybe<T[]> List<T>(this Maybe<TextParser> p, ReadOnlySpan<char> separator, Func<TextParser, Maybe<T>> itemParser)
            => p ? List(p.Value, separator, itemParser) : p.Convert<T[]>();

        public static Maybe<IReportNode[]> List(this TextParser p, ReadOnlySpan<char> separator, IReportParser itemParser)
            => List(p, separator, x => itemParser.Parse(x));

        public static Maybe<IReportNode[]> List(this Maybe<TextParser> p, ReadOnlySpan<char> separator, IReportParser itemParser)
            => p ? List(p.Value, separator, itemParser) : p.Convert<IReportNode[]>();

        public static Maybe<T> RecoverWith<T>(this Maybe<T> p, Func<T> onFailure) => p ? p : new Maybe<T>(onFailure());

        public static Maybe<P> Map<T, P>(this Maybe<T> p, Func<T, P> mapping) => p ? new Maybe<P>(mapping(p.Value)) : p.Convert<P>();

        public static Maybe<TextParser> Then(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Then(s) : p;

        public static bool Contains(this Maybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Contains(s) : false;
    }
}
