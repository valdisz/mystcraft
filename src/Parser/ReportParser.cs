namespace atlantis {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class ReportParser : IReportParser {
        public Maybe<IReportNode> Parse(TextParser p) => p.Try(Execute);

        protected abstract Maybe<IReportNode> Execute(TextParser p);

        protected Maybe<IReportNode> Ok(IReportNode result) => new Maybe<IReportNode>(result);

        protected Maybe<IReportNode> Error<T>(Maybe<T> p) => p.Convert<IReportNode>();

        protected Maybe<TextParser> LastResult;

        protected Maybe<TextParser> Mem(Maybe<TextParser> result) {
            LastResult = result;
            return result;
        }
    }

    public interface IReportParser {
        Maybe<IReportNode> Parse(TextParser p);
    }

    public interface IReportNode {
        string Type { get; }
        bool HasChildren { get; }
        IList<IReportNode> Children { get; }

        void Add(IReportNode child);
        void AddRange(IEnumerable<IReportNode> children);
        void AddRange(params IReportNode[] children);
    }

    public abstract class ReportNode : IReportNode {
        protected ReportNode(string type, bool hasChildren) {
            Type = type;
            HasChildren = hasChildren;

            if (hasChildren) Children = new List<IReportNode>();
        }

        public string Type { get; }
        public bool HasChildren { get; }
        public IList<IReportNode> Children { get; }

        public virtual void Add(IReportNode child) {
            if (HasChildren && child != null) {
                if (child is ReportBag) {
                    AddMany(child.Children);
                }
                else {
                    Children.Add(child);
                }
            }
        }

        protected virtual void AddMany(IEnumerable<IReportNode> children) {
            if (HasChildren) {
                foreach (var child in children) {
                    if (child != null) {
                        if (child is ReportBag) {
                            AddMany(child.Children);
                        }
                        else {
                            Children.Add(child);
                        }
                    }
                }
            }
        }

        public void AddRange(IEnumerable<IReportNode> children) => AddMany(children);
        public void AddRange(params IReportNode[] children)  => AddMany(children);

        public static IReportNode Array(params IReportNode[] children) => new ReportArray(children);
        public static IReportNode Array(IEnumerable<IReportNode> children) => new ReportArray(children.ToArray());
        public static IReportNode Object(params IReportNode[] children) => new ReportObject(children);
        public static IReportNode Object(IEnumerable<IReportNode> children) => new ReportObject(children.ToArray());
        public static IReportNode Bag(params IReportNode[] children) => new ReportBag(children);
        public static IReportNode Key(string key, IReportNode value) => new ReportKey(key, value);
        public static IReportNode Int(string key, int value) => new ReportKey(key, new ReportInt(value));
        public static IReportNode Str(string key, string value) => new ReportKey(key, new ReportStr(value));
        public static IReportNode Bool(string key, bool value) => new ReportKey(key, new ReportBool(value));
        public static IReportNode Null(string key) => new ReportKey(key, new ReportNull());
        public static IReportNode Int(int value) => new ReportInt(value);
        public static IReportNode Str(string value) => new ReportStr(value);
        public static IReportNode Null() => new ReportNull();
    }

    public class ReportArray : ReportNode {
        public ReportArray(params IReportNode[] children) : base("array", true) {
            if (children != null) AddRange(children);
        }
    }

    public class ReportObject : ReportNode {
        public ReportObject(params IReportNode[] children) : base("object", true) {
            if (children != null) AddRange(children);
        }
    }

    public class ReportBag : ReportNode {
        public ReportBag(params IReportNode[] children) : base("bag", true) {
            if (children != null) AddRange(children);
        }
    }

    public class ReportKey : ReportNode {
        public ReportKey(string key, IReportNode value) : base(key, false) {
            Value = value;
        }

        public IReportNode Value { get; }
    }

    public class ReportInt : ReportNode
    {
        public ReportInt(int value) : base("int", false)
        {
            Value = value;
        }

        public int Value { get; }
    }

    public class ReportStr : ReportNode
    {
        public ReportStr(string value) : base("str", false)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public class ReportBool : ReportNode
    {
        public ReportBool(bool value) : base("bool", false)
        {
            Value = value;
        }

        public bool Value { get; }
    }

    public class ReportNull : ReportNode
    {
        public ReportNull() : base("null", false)
        {
        }
    }
}
