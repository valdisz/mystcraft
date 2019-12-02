namespace atlantis {
    public interface IReportParser {
        Maybe<IReportNode> Parse(TextParser p);
    }

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
}
