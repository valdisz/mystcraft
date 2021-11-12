namespace advisor
{
    using System.Collections.Generic;

    public class ItemListParser : BaseParser {
        readonly IReportParser itemParser = new ItemParser();

        protected override Maybe<IReportNode> Execute(TextParser p) {
            if (p.Match("none")) {
                return Ok(ReportNode.Array());
            }

            var items = new List<IReportNode>();
            while (!p.EOF) {
                if (items.Count > 0) {
                    Maybe<TextParser> result = p.After(",").SkipWhitespaces();
                    if (!result) {
                        return Error(result);
                    }
                }

                var item = itemParser.Parse(p);
                if (!item) {
                    return Error(item);
                }

                items.Add(item.Value);
            }

            return Ok(ReportNode.Array(items));
        }
    }
}
