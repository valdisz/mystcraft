namespace atlantis
{
    using System;

    public class ReportDateParser : BaseParser {
        protected override Maybe<IReportNode> Execute(TextParser p) {
            var month = p.Before(",").AsString();
            if (!month) return Error(month);

            var year = p.After("Year").SkipWhitespaces().Integer();
            if (!year) return Error(year);

            return Ok(ReportNode.Bag(
                ReportNode.Key("date", ReportNode.Object(
                    ReportNode.Int("month", MonthToNumber(month)),
                    ReportNode.Int("year", year)
                ))
            ));
        }

        private int MonthToNumber(string monthName) {
            switch (monthName.ToLowerInvariant()) {
                case "january": return 1;
                case "february": return 2;
                case "march": return 3;
                case "april": return 4;
                case "may": return 5;
                case "june": return 6;
                case "july": return 7;
                case "august": return 8;
                case "september": return 9;
                case "october": return 10;
                case "november": return 11;
                case "december": return 12;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
