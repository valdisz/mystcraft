namespace advisor {
    public interface IParser {
        PMaybe<IReportNode> Parse(TextParser p);
    }

    public abstract class BaseParser : IParser {
        public PMaybe<IReportNode> Parse(TextParser p) => p.Try(Execute);

        protected abstract PMaybe<IReportNode> Execute(TextParser p);

        protected PMaybe<IReportNode> Ok(IReportNode result) => new PMaybe<IReportNode>(result);

        protected PMaybe<IReportNode> Error<T>(PMaybe<T> p) => p.Convert<IReportNode>();

        protected PMaybe<TextParser> LastResult;

        protected PMaybe<TextParser> Mem(PMaybe<TextParser> result) {
            LastResult = result;
            return result;
        }
    }

    public static class ReportParserExtensions {
        public static PMaybe<IReportNode> ParseMaybe(this IParser parser, PMaybe<TextParser> p)
            => p ? parser.Parse(p.Value) : p.Convert<IReportNode>();
    }
}
