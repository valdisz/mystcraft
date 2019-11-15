namespace atlantis
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Newtonsoft.Json;
    using Pidgin;
    using ParserNode = Pidgin.Parser<char, IReportNodeOld>;

    public interface IReportNodeOld
    {
        string Type { get; }
        bool HasChildren { get; }
        IList<IReportNodeOld> Children { get; }

        void Add(IReportNodeOld child);
        void AddRange(IEnumerable<IReportNodeOld> children);
        void AddRange(params IReportNodeOld[] children);

        IReportNodeOld[] ByType(string type);

        void WriteJson(JsonWriter writer);
    }

    public static class ReportNodeExtensions {
        public static string StrValueOf(this IReportNodeOld node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return (nodes[0] as ValueReportNode<string>)?.Value;
        }

        public static int? IntValueOf(this IReportNodeOld node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return (nodes[0] as ValueReportNode<int>)?.Value;
        }

        public static double? RealValueOf(this IReportNodeOld node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return (nodes[0] as ValueReportNode<double>)?.Value;
        }


        public static string StrValue(this IReportNodeOld node) {
            return (node as ValueReportNode<string>)?.Value;
        }

        public static int? IntValue(this IReportNodeOld node) {
            return (node as ValueReportNode<int>)?.Value;
        }

        public static double? RealValue(this IReportNodeOld node) {
            return (node as ValueReportNode<double>)?.Value;
        }

        public static IReportNodeOld FirstByType(this IReportNodeOld node, string type) {
            var nodes = node.ByType(type);
            if (nodes.Length == 0) {
                return null;
            }

            return nodes[0];
        }
    }

    public abstract class BaseReportNodeOld : IReportNodeOld
    {
        public abstract string Type { get; }
        public abstract bool HasChildren { get; }
        public abstract IList<IReportNodeOld> Children { get; }

        public virtual void Add(IReportNodeOld child)
        {
            if (HasChildren) Children.Add(child);
        }

        protected virtual void AddMany(IEnumerable<IReportNodeOld> children) {
            if (HasChildren) {
                foreach (var child in children) {
                    Children.Add(child);
                }
            }
        }

        public void AddRange(IEnumerable<IReportNodeOld> children) => AddMany(children);
        public void AddRange(params IReportNodeOld[] children)  => AddMany(children);

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

        public IReportNodeOld[] ByType(string type)
        {
            if (!HasChildren) {
                throw new InvalidOperationException();
            }

            return Children.Where(x => x.Type == type).ToArray();
        }

        public abstract void WriteJson(JsonWriter writer);
    }

    public class ValueReportNode<T> : BaseReportNodeOld
    {
        public ValueReportNode(string type, T value, bool writeProperty =  true) {
            this.type = type;
            this.writeProperty = writeProperty;
            Value = value;
        }

        private readonly string type;
        private readonly bool writeProperty;

        public override string Type => type;
        public override bool HasChildren => false;
        public override IList<IReportNodeOld> Children => throw new NotImplementedException();

        public T Value { get; }

        public override string ToString() => $"{Type} ({Value})";

        public override void WriteJson(JsonWriter writer)
        {
            if(writeProperty) writer.WritePropertyName(Type);
            writer.WriteValue(Value);
        }
    }

    public class ReportNodeOld : BaseReportNodeOld
    {
        public ReportNodeOld(string type, bool asArray, bool writeProperty, params IReportNodeOld[] nodes) {
            this.type = type;
            this.asArray = asArray;
            this.writeProperty = writeProperty;
            if (nodes != null && nodes.Length > 0) {
                children.AddRange(nodes);
            }
        }

        private readonly string type;
        private readonly bool asArray;
        private readonly bool writeProperty;
        private readonly List<IReportNodeOld> children = new List<IReportNodeOld>();

        public override string Type => type;
        public override bool HasChildren => true;
        public override IList<IReportNodeOld> Children => children;

        public override void WriteJson(JsonWriter writer)
        {
            if (writeProperty) writer.WritePropertyName(Type);
            if (asArray)
                writer.WriteStartArray();
            else
                writer.WriteStartObject();

            foreach (var child in children) {
                child.WriteJson(writer);
            }

            if (asArray)
                writer.WriteEndArray();
            else
                writer.WriteEndObject();
        }
    }

    public static class Nodes {
        public static IReportNodeOld Node(string type, bool asArray, bool writeProperty, params IReportNodeOld[] children) {
            return new ReportNodeOld(type, asArray, writeProperty, children);
        }

        public static IReportNodeOld Node(string type, bool asArray, bool writeProperty, IEnumerable<IReportNodeOld> children) {
            return new ReportNodeOld(type, asArray, writeProperty, (children ?? Enumerable.Empty<IReportNodeOld>()).ToArray());
        }

        public static IReportNodeOld Node(string type, bool asArray, bool writeProperty, IEnumerable<Maybe<IReportNodeOld>> children) {
            return new ReportNodeOld(
                type,
                asArray,
                writeProperty,
                (children ?? Enumerable.Empty<Maybe<IReportNodeOld>>())
                    .Where(x => x.Success)
                    .Select(x => x.Value)
                    .ToArray()
            );
        }

        public static IReportNodeOld Str(string type, string value, bool writeProperty = true) {
            return new ValueReportNode<string>(type, value.Trim(), writeProperty);
        }

        public static IReportNodeOld Num(string type, int value, bool writeProperty = true) {
            return new ValueReportNode<int>(type, value, writeProperty);
        }

        public static IReportNodeOld Num(string type, string value, bool writeProperty = true) {
            return new ValueReportNode<int>(type, int.Parse(value), writeProperty);
        }

        public static IReportNodeOld Float(string type, double value, bool writeProperty = true) {
            return new ValueReportNode<double>(type, value, writeProperty);
        }

        public static IReportNodeOld Float(string type, string value, bool writeProperty = true) {
            return new ValueReportNode<double>(type, double.Parse(value, CultureInfo.InstalledUICulture), writeProperty);
        }

        public static ParserNode Node(this Parser<char, IEnumerable<IReportNodeOld>> parser, string type, bool asArray, bool writeProperty) {
            return parser.Select(x => Node(type, asArray, writeProperty, x));
        }

        public static ParserNode Node(this Parser<char, IEnumerable<Maybe<IReportNodeOld>>> parser, string type, bool asArray, bool writeProperty) {
            return parser.Select(x => Node(type, asArray, writeProperty, x.Where(node => node.Success).Select(node => node.Value)));
        }

        public static ParserNode Str(this Parser<char, string> parser, string type, bool writeProperty = true) {
            return parser.Select(x => Str(type, x, writeProperty));
        }

        public static ParserNode Num(this Parser<char, string> parser, string type, bool writeProperty = true) {
            return parser.Select(x => Num(type, x, writeProperty));
        }

        public static ParserNode Num(this Parser<char, int> parser, string type, bool writeProperty = true) {
            return parser.Select(x => Num(type, x, writeProperty));
        }

        public static ParserNode Float(this Parser<char, string> parser, string type, bool writeProperty = true) {
            return parser.Select(x => Float(type, x, writeProperty));
        }

        public static ParserNode Float(this Parser<char, double> parser, string type, bool writeProperty = true) {
            return parser.Select(x => Float(type, x, writeProperty));
        }
    }
}
