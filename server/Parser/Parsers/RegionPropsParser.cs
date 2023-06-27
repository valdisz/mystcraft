using System.Collections.Generic;

namespace advisor
{
    public class RegionPropsParser : BaseParser {
        public RegionPropsParser(ItemParser itemParser) {
            this.itemParser = itemParser;
        }

        private readonly ItemParser itemParser;

        private PMaybe<IReportNode> ParaseItems(TextParser src, IParser parser, List<IReportNode> items) {
            if (src.Match("none")) {
                return null;
            }

            while (!src.EOF) {
                if (items.Count > 0) {
                    PMaybe<TextParser> result = src.After(",").SkipWhitespaces();
                    if (!result) {
                        return Error(result);
                    }
                }

                var item = itemParser.Parse(src);
                if (!item) {
                    return Error(item);
                }

                items.Add(item.Value);
            }

            return null;
        }

        protected override PMaybe<IReportNode> Execute(TextParser p) {
            PMaybe<double> wages = null;
            PMaybe<int> totalWages = null;
            List<IReportNode> wanted = new List<IReportNode>();
            List<IReportNode> forSale = new List<IReportNode>();
            PMaybe<int> entertainment = null;
            List<IReportNode> products = new List<IReportNode>();

            p.Before("Wages:");

            while (!p.EOF) {
                var prop = p.SkipWhitespaces().Before(":").AsString();
                if (!prop) return Error(prop);

                p.After(":").SkipWhitespaces();

                PMaybe<TextParser> value;
                switch (prop.Value) {
                    case "Wages": {
                        value = p.OneOf(
                            x => x.Before(")."),
                            x => x.Match("$0.")
                        )
                        .SkipWhitespaces();
                        break;
                    }

                    default: {
                        value = p.Before(".").SkipWhitespaces();
                        break;
                    }
                }
                if (!value) {
                    return Error(value);
                }

                p.After(".");

                switch (prop.Value) {
                    case "Wages": {
                        wages = value.Value.EOF
                            ? new PMaybe<double>(0)
                            : value.Seek(1).Real();
                        if (!wages) {
                            return Error(wages);
                        }

                        if (value.After("(Max:").SkipWhitespaces()) {
                            totalWages = value.Seek(1).Integer();
                            if (!totalWages) {
                                return Error(wages);
                            }
                        }

                        break;
                    }

                    case "Wanted": {
                        var err = ParaseItems(value.Value, itemParser, wanted);
                        if (err != null) {
                            return err;
                        }
                        break;
                    }

                    case "For Sale": {
                        var err = ParaseItems(value.Value, itemParser, forSale);
                        if (err != null) {
                            return err;
                        }
                        break;
                    }

                    case "Products": {
                        var err = ParaseItems(value.Value, itemParser, products);
                        if (err != null) {
                            return err;
                        }
                        break;
                    }

                    case "Entertainment available": {
                        entertainment = value.Seek(1).Integer();
                        if (!entertainment) {
                            return Error(entertainment);
                        }
                        break;
                    }

                    default: {
                        continue;
                    }
                }
            }

            return Ok(ReportNode.Bag(
                ReportNode.Real("wages", wages?.Value ?? 0),
                ReportNode.Int("totalWages", totalWages?.Value ?? 0),
                ReportNode.Key("wanted", ReportNode.Array(wanted)),
                ReportNode.Key("forSale", ReportNode.Array(forSale)),
                ReportNode.Int("entertainment", entertainment?.Value ?? 0),
                ReportNode.Key("products", ReportNode.Array(products))
            ));
        }
    }
}
