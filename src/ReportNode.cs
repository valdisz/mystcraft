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
    using ParserNode = Pidgin.Parser<char, IReportNode>;

    public interface IReportNode
    {
        string Type { get; }
        bool HasChildren { get; }
        IList<IReportNode> Children { get; }

        void Add(IReportNode child);
        void AddRange(IEnumerable<IReportNode> children);
        void AddRange(params IReportNode[] children);

        IReportNode[] ByType(string type);

        void WriteJson(JsonWriter writer);
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

        public abstract void WriteJson(JsonWriter writer);
    }

    public class ValueReportNode<T> : BaseReportNode
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
        public override IList<IReportNode> Children => throw new NotImplementedException();

        public T Value { get; }

        public override string ToString() => $"{Type} ({Value})";

        public override void WriteJson(JsonWriter writer)
        {
            if(writeProperty) writer.WritePropertyName(Type);
            writer.WriteValue(Value);
        }
    }

    public class ReportNode : BaseReportNode
    {
        public ReportNode(string type, bool asArray, bool writeProperty, params IReportNode[] nodes) {
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
        private readonly List<IReportNode> children = new List<IReportNode>();

        public override string Type => type;
        public override bool HasChildren => true;
        public override IList<IReportNode> Children => children;

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
        public static IReportNode Node(string type, bool asArray, bool writeProperty, params IReportNode[] children) {
            return new ReportNode(type, asArray, writeProperty, children);
        }

        public static IReportNode Node(string type, bool asArray, bool writeProperty, IEnumerable<IReportNode> children) {
            return new ReportNode(type, asArray, writeProperty, (children ?? Enumerable.Empty<IReportNode>()).ToArray());
        }

        public static IReportNode Node(string type, bool asArray, bool writeProperty, IEnumerable<Maybe<IReportNode>> children) {
            return new ReportNode(
                type,
                asArray,
                writeProperty,
                (children ?? Enumerable.Empty<Maybe<IReportNode>>())
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToArray()
            );
        }

        public static IReportNode Str(string type, string value, bool writeProperty = true) {
            return new ValueReportNode<string>(type, value.Trim(), writeProperty);
        }

        public static IReportNode Num(string type, int value, bool writeProperty = true) {
            return new ValueReportNode<int>(type, value, writeProperty);
        }

        public static IReportNode Num(string type, string value, bool writeProperty = true) {
            return new ValueReportNode<int>(type, int.Parse(value), writeProperty);
        }

        public static IReportNode Float(string type, double value, bool writeProperty = true) {
            return new ValueReportNode<double>(type, value, writeProperty);
        }

        public static IReportNode Float(string type, string value, bool writeProperty = true) {
            return new ValueReportNode<double>(type, double.Parse(value, CultureInfo.InstalledUICulture), writeProperty);
        }


        public static ParserNode Node(this Parser<char, IEnumerable<IReportNode>> parser, string type, bool asArray, bool writeProperty) {
            return parser.Select(x => Node(type, asArray, writeProperty, x));
        }

        public static ParserNode Node(this Parser<char, IEnumerable<Maybe<IReportNode>>> parser, string type, bool asArray, bool writeProperty) {
            return parser.Select(x => Node(type, asArray, writeProperty, x.Where(node => node.HasValue).Select(node => node.Value)));
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
