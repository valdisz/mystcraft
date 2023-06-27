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

        public PMaybe<TextParser> Seek(int n) {
            var newPos = Pos + n;
            if (newPos < 0 || newPos > Text.Length) {
                return new PMaybe<TextParser>("EOF", Ln, Pos + 1);
            }

            Pos = newPos;
            return new PMaybe<TextParser>(this);
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

        public PMaybe<TextParser> Slice(int length) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var s = new TextParser(Ln, Text.Slice(Pos, length));
            Seek(length);
            return new PMaybe<TextParser>(s);
        }

        public PMaybe<TextParser> Before(ReadOnlySpan<char> s) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var i = LFind(s);
            if (i >= 0) {
                return new PMaybe<TextParser>(Slice(i));
            }

            return new PMaybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public PMaybe<TextParser> Before(params string[] list) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            int i = -1;
            foreach (var item in list) {
                var j = LFind(item);
                if (j < 0) continue;

                if (i < 0 || j < i) {
                    i = j;
                }
            }

            if (i >= 0) {
                return new PMaybe<TextParser>(Slice(i));
            }

            return new PMaybe<TextParser>($"Cant find any of {string.Join(",", list)}", Ln, Pos + 1);
        }

        public PMaybe<TextParser> After(ReadOnlySpan<char> s) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var i = LFind(s);
            if (i >= 0) {
                Seek(i + s.Length);
                return new PMaybe<TextParser>(this);
            }

            return new PMaybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public PMaybe<TextParser> BeforeBackwards(ReadOnlySpan<char> s) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var i = RFind(s);
            if (i >= 0) {
                return new PMaybe<TextParser>(Slice(i));
            }

            return new PMaybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public PMaybe<TextParser> AfterBackwards(ReadOnlySpan<char> s) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var i = RFind(s);
            if (i >= 0) {
                Seek(i + s.Length);
                return new PMaybe<TextParser>(this);
            }

            return new PMaybe<TextParser>($"Cant find {s.ToString()}", Ln, Pos + 1);
        }

        public PMaybe<TextParser> SkipWhitespaces(int minTimes = 0) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = 0; i < span.Length; i++) {
                if (!char.IsWhiteSpace(span[i])) {
                    if (i < minTimes) {
                        return new PMaybe<TextParser>($"Whitespace must present at least {minTimes} times", Ln, Pos + 1);
                    }

                    break;
                }
            }

            return Seek(i);
        }

        public PMaybe<TextParser> SkipWhitespacesBackwards(int minTimes = 0) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = span.Length - 1; i >= 0; i--) {
                if (!char.IsWhiteSpace(span[i])) {
                    if ((span.Length - i - 1) < minTimes) {
                        return new PMaybe<TextParser>($"Whitespace must present at least {minTimes} times", Ln, Pos + 1);
                    }

                    break;
                }
            }

            return Slice(i + 1);
        }

        public PMaybe<TextParser> SkipChar(char c) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = 0; i < span.Length; i++) {
                if (span[i] != c) {
                    break;
                }
            }

            return Seek(i);
        }

        public PMaybe<TextParser> Skip(Func<char, bool> predicate) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            int i;
            for (i = 0; i < span.Length; i++) {
                if (!predicate(span[i])) {
                    break;
                }
            }

            return Seek(i);
        }

        public PMaybe<TextParser> Then(ReadOnlySpan<char> s) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            if (span.Length < s.Length || !span.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                return new PMaybe<TextParser>($"'{span.ToString()}' does not match '{s.ToString()}'", Ln, Pos + 1);

            return Seek(s.Length);
        }

        public PMaybe<int> Integer() {
            if (EOF) return new PMaybe<int>("EOF", Ln, Pos + 1);

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
                return new PMaybe<int>("Not a number", Ln, Pos + 1);
            }

            for (; i < span.Length; i++) {
                if (!char.IsDigit(span[i])) {
                    break;
                }
            }

            if (i < 0) {
                PopBookmark();
                return new PMaybe<int>("Expected +, - or number", Ln, Pos + 1);
            }

            RemoveBookmark();

            var s = Slice(i);
            if (!s) return s.Convert<int>();

            return new PMaybe<int>(int.Parse(s.Value.Text.Span));
        }

        public PMaybe<double> Real() {
            if (EOF) return new PMaybe<double>("EOF", Ln, Pos + 1);

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
                return new PMaybe<double>("Not a number", Ln, Pos + 1);
            }

            for (; i < span.Length; i++) {
                if (!char.IsDigit(span[i]) && span[i] != '.') {
                    break;
                }
            }

            if (i < 0) {
                PopBookmark();
                return new PMaybe<double>("Expected +, -, . or number", Ln, Pos + 1);
            }

            RemoveBookmark();

            var s = Slice(i);
            if (!s) return s.Convert<double>();

            return new PMaybe<double>(double.Parse(s.Value.Text.Span));
        }

        public PMaybe<TextParser> Word() {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            PushBookmark();
            SkipWhitespaces();

            if (EOF) {
                PopBookmark();
                return new PMaybe<TextParser>("Reached EOF", Ln, Pos + 1);
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
                return new PMaybe<TextParser>("Not a word", Ln, Pos + 1);
            }

            RemoveBookmark();
            return new PMaybe<TextParser>(Slice(i));
        }

        public PMaybe<string[]> Words() {
            if (EOF) return new PMaybe<string[]>("EOF", Ln, Pos + 1);

            PushBookmark();

            var first = Word();
            if (!first) {
                PopBookmark();
                return first.Convert<string[]>();
            }

            // todo: w.Value.Length == 0 is workardound
            if (first.Value.Length == 0) {
                PopBookmark();
                return new PMaybe<string[]>("Not a word", Ln, Pos);
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
            return new PMaybe<string[]>(words.ToArray());
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

        public PMaybe<TextParser> Match(ReadOnlySpan<char> s) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            if (span.Length < s.Length || !span.StartsWith(s, StringComparison.OrdinalIgnoreCase))
                return new PMaybe<TextParser>($"'{span.ToString()}' does not match '{s.ToString()}'", Ln, Pos + 1);

            return Slice(s.Length);
        }

        public PMaybe<TextParser> MatchEnd(ReadOnlySpan<char> s) {
            if (EOF) return new PMaybe<TextParser>("EOF", Ln, Pos + 1);

            var span = GetSpan();
            if (span.Length < s.Length || !span.EndsWith(s, StringComparison.OrdinalIgnoreCase))
                return new PMaybe<TextParser>($"'{span.ToString()}' does not match '{s.ToString()}'", Ln, Pos + 1);

            return Slice(span.Length - s.Length);
        }

        public PMaybe<TextParser> Between(ReadOnlySpan<char> left, ReadOnlySpan<char> right, bool useSkip = false) {
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

        public PMaybe<TextParser> Between(ReadOnlySpan<char> s, bool useSkip = false) => Between(s, s, useSkip);

        public override string ToString() => GetSpan().ToString();

        public string AsString() => ToString();

        public static implicit operator string(TextParser p) => p.ToString();
    }

    public static class TextParserExtensions {
        public static PMaybe<TextParser> Reset(this PMaybe<TextParser> p) => p ? new PMaybe<TextParser>(p.Value.Reset()) : p;
        public static PMaybe<TextParser> Seek(this PMaybe<TextParser> p, int n) => p ? new PMaybe<TextParser>(p.Value.Seek(n)) : p;

        public static PMaybe<TextParser> Before(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Before(s) : p;
        public static PMaybe<TextParser> After(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.After(s) : p;
        public static PMaybe<TextParser> BeforeBackwards(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.BeforeBackwards(s) : p;
        public static PMaybe<TextParser> AfterBackwards(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.AfterBackwards(s) : p;

        public static PMaybe<TextParser> Before(this PMaybe<TextParser> p, params string[] list) => p ? p.Value.Before(list) : p;

        public static PMaybe<TextParser> After(this TextParser p, params string[] list) {
            foreach (var s in list) {
                var match = p.After(s);
                if (match) return match;
            }

            return new PMaybe<TextParser>($"all options does ({string.Join(", ", list)}) not fit", p.Ln, p.Pos + 1);
        }

        public static PMaybe<TextParser> SkipWhitespaces(this PMaybe<TextParser> p, int minTimes = 0) {
            if (!p) return p;
            return p.Value.SkipWhitespaces(minTimes);
        }

        public static PMaybe<TextParser> SkipWhitespacesBackwards(this PMaybe<TextParser> p, int minTimes = 0) {
            if (!p) return p;
            return p.Value.SkipWhitespacesBackwards(minTimes);
        }

        public static PMaybe<TextParser> Word(this PMaybe<TextParser> p) => p ? p.Value.Word() : p;
        public static PMaybe<TextParser> Match(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Match(s) : p;
        public static PMaybe<TextParser> MatchEnd(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.MatchEnd(s) : p;
        public static bool StartsWith(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.StartsWith(s) : false;
        public static PMaybe<int> Integer(this PMaybe<TextParser> p) => p ? p.Value.Integer() : p.Convert<int>();
        public static PMaybe<double> Real(this PMaybe<TextParser> p) => p ? p.Value.Real() : p.Convert<double>();
        public static PMaybe<TextParser> Between(this PMaybe<TextParser> p, ReadOnlySpan<char> left, ReadOnlySpan<char> right, bool useThen = false) => p ? p.Value.Between(left, right, useThen) : p;
        public static PMaybe<TextParser> Between(this PMaybe<TextParser> p, ReadOnlySpan<char> s, bool useThen = false) => p ? p.Value.Between(s, useThen) : p;
        public static PMaybe<string> AsString(this PMaybe<TextParser> p) => p ? new PMaybe<string>(p.Value) : p.Convert<string>();

        public static PMaybe<T> Try<T>(this PMaybe<TextParser> p, Func<TextParser, PMaybe<T>> parser)
            => p ? Try(p.Value, parser) : p.Convert<T>();

        public static PMaybe<IReportNode> Try(this TextParser parser, IParser reportParser)
            => Try(parser, reportParser.Parse);

        public static PMaybe<IReportNode> Try(this PMaybe<TextParser> parser, IParser reportParser)
            => Try(parser, reportParser.Parse);

        public static PMaybe<T> Try<T>(this TextParser p, Func<TextParser, PMaybe<T>> parser) {
            p.PushBookmark();
            PMaybe<T> result = parser(p);

            if (!result) {
                p.PopBookmark();
            }
            else {
                p.RemoveBookmark();
            }

            return result;
        }

        public static PMaybe<T> OneOf<T>(this PMaybe<TextParser> p, params Func<TextParser, PMaybe<T>>[] parsers)
            => p ? OneOf(p.Value, parsers) : p.Convert<T>();

        public static PMaybe<T> OneOf<T>(this TextParser p, params Func<TextParser, PMaybe<T>>[] parsers) {
            for (var i = 0; i < parsers.Length; i++) {
                var result = p.Try(parsers[i]);
                if (result) return result;
            }

            return new PMaybe<T>("OneOf: all options does not fit", p.Ln, p.Pos + 1);
        }

        public static PMaybe<IReportNode> OneOf(this PMaybe<TextParser> p, params IParser[] parsers)
            => p ? OneOf(p.Value, parsers) : p.Convert<IReportNode>();

        public static PMaybe<TextParser> OneOf(this TextParser p, params string[] str) {
            for (var i = 0; i < str.Length; i++) {
                var result = p.Try(parser => parser.Match(str[i]));
                if (result) return result;
            }

            return new PMaybe<TextParser>($"OneOf: all options ({string.Join(", ", str)}) does not fit", p.Ln, p.Pos + 1);
        }

        public static PMaybe<TextParser> OneOf(this PMaybe<TextParser> p, params string[] str)
            => p ? OneOf(p.Value, str) : p.Convert<TextParser>();

        public static PMaybe<IReportNode> OneOf(this TextParser p, params IParser[] parsers) {
            for (var i = 0; i < parsers.Length; i++) {
                var result = p.Try(parsers[i]);
                if (result) return result;
            }

            return new PMaybe<IReportNode>("OneOf: all options does not fit", p.Ln, p.Pos + 1);
        }

        public static PMaybe<T[]> List<T>(this TextParser p, ReadOnlySpan<char> separator, Func<TextParser, PMaybe<T>> itemParser) {
            List<T> items = new List<T>();

            if (p.Match("none")) {
                return new PMaybe<T[]>(new T[0]);
            }

            while (!p.EOF) {
                PMaybe<TextParser> span = p.Before(separator);
                if (span) p.Seek(separator.Length);

                var item = itemParser(span ? span : p);
                if (!item) return item.Convert<T[]>();

                items.Add(item.Value);
                if (!span) {
                    break;
                }
            }

            return new PMaybe<T[]>(items.ToArray());
        }

        public static PMaybe<T[]> List<T>(this PMaybe<TextParser> p, ReadOnlySpan<char> separator, Func<TextParser, PMaybe<T>> itemParser)
            => p ? List(p.Value, separator, itemParser) : p.Convert<T[]>();

        public static PMaybe<IReportNode[]> List(this TextParser p, ReadOnlySpan<char> separator, IParser itemParser)
            => List(p, separator, x => itemParser.Parse(x));

        public static PMaybe<IReportNode[]> List(this PMaybe<TextParser> p, ReadOnlySpan<char> separator, IParser itemParser)
            => p ? List(p.Value, separator, itemParser) : p.Convert<IReportNode[]>();

        public static PMaybe<T> RecoverWith<T>(this PMaybe<T> p, Func<T> onFailure) => p ? p : new PMaybe<T>(onFailure());

        public static PMaybe<P> Map<T, P>(this PMaybe<T> p, Func<T, P> mapping) => p ? new PMaybe<P>(mapping(p.Value)) : p.Convert<P>();

        public static PMaybe<TextParser> Then(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Then(s) : p;

        public static bool Contains(this PMaybe<TextParser> p, ReadOnlySpan<char> s) => p ? p.Value.Contains(s) : false;
    }
}
