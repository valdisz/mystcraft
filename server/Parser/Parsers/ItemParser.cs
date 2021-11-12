using System.Linq;
using System.Text.RegularExpressions;

namespace advisor
{
    // high elf [HELF]
    // 2 high elves [HELF]
    // unlimited high elves [HELF]
    // high elf [HELF] at $34
    // 2 high elves [HELF] at $48
    // unlimited high elves [HELF] at $75
    public class ItemParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var amount = p.Try(pp => pp.Word().OneOf(
                _ => _.Match("unlimited").Map(_ => -1),
                _ => _.Integer()
            ));

            var name = p.Words().Map(words => string.Join(" ", words));
            if (!name) return Error(name);

            var code = p.Skip(" ").SkipWhitespaces().Between("[", "]").AsString();
            if (!code) return Error(code);

            var propOrPrice = p.Try(_ => _.OneOf(
                pp => pp
                    .Skip(" ")
                    .SkipWhitespaces()
                    .Between("(", ")")
                    .AsString()
                    .Map(props => ReportNode.Str("props", props)),
                pp => pp
                    .After(" at ")
                    .SkipWhitespaces()
                    .Seek(1)
                    .Word()
                    .Integer()
                    .Map(price => ReportNode.Int("price", price))
            ));

            return Ok(ReportNode.Object(
                ReportNode.Int("amount", amount ? amount.Value : 1),
                ReportNode.Str("name", name),
                ReportNode.Str("code", code),
                propOrPrice ? propOrPrice.Value : null
            ));
        }
    }
}
