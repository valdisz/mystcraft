using System.Collections.Generic;
using advisor.Model;

namespace advisor {
    // Soldiers (697): Collects $100 in taxes in mountain (49,39) in Swerthenchi.
    // Unit (1122): Sells 4 jewelry [JEWE] at $772 each.
    // Scout (672): Earns 7 silver working in mountain (48,40) in Swerthenchi.
    // P GRAI (1420): Produces 9 grain [GRAI] in desert (51,43) in Osdhan.
    // Raiders (908): Pillages $1116 from mountain (48,40) in Swerthenchi.

    // P STONE (980): Claims $12.
    // Unit (2243) is caught attempting to steal from Unit (1918) in Shicarpen.
    // Imperial Guards (1009): Gives 40 silver [SILV] to Contractors (704).
    // Unit (2556) pillages Swerthenchi.
    // Gardan (648) uses illusion [ILLU] in desert (49,45) in Osdhan.
    // new guards (656): Buys 14 humans [HUMN] at $42 each.
    // Flagship of the Swerthenchi Alliance [104] sails from desert (49,45) in Osdhan to ocean (49,47) in Quui'udthrar Sea.
    // Courier (990): Rides from plain (52,44) in Shicarpen to plain (52,46) in Shicarpen.
    // Scout (905): Walks from plain (52,46) in Shicarpen to plain (52,44) in Shicarpen.
    // Aquatic Scout (1324): Swims from ocean (57,57) in Lechcobon Sea to ocean (58,58) in Lechcobon Sea.
    // Aquatic Scout (1324): MOVE: Unit has insufficient movement points; remaining moves queued.
    // Emperor (644): Teaches force to Mage 4 (993).
    // new guards (656): Studies combat.
    // Mage 2 (1100): Studies force and was taught for 30 days.
    // Scout (672): Claims 2 silver for maintenance.
    public class EventParser : BaseParser {
        public EventParser(UnitNameParser unitNameParser, LocationParser locationParser, ItemParser itemParser) {
            this.unitNameParser = unitNameParser;
            this.locationParser = locationParser;
            this.itemParser = itemParser;
        }

        private readonly UnitNameParser unitNameParser;
        private readonly LocationParser locationParser;
        private readonly ItemParser itemParser;

        protected override Maybe<IReportNode> Execute(TextParser p) {
            var result = p.Try<IReportNode>(msg => {
                var unit = unitNameParser.Parse(msg);
                if (!unit) return Error(unit);

                var message = msg.After(":").SkipWhitespaces().AsString();
                if (!message) return Error(message);

                var keyword = msg.Word().AsString();
                if (!keyword) return Error(keyword);

                msg.SkipWhitespaces();

                Maybe<IReportNode> location = null;
                Maybe<int> amount = null;
                Maybe<IReportNode> item = null;

                EventCategory category;
                switch (keyword.ToString().ToLower()) {
                    case "collects": {
                        category = EventCategory.Tax;
                        amount = Money(msg);
                        location = locationParser.Parse(msg
                            .After("in").SkipWhitespaces()
                            .After("taxes").SkipWhitespaces()
                            .After("in").SkipWhitespaces()
                            .BeforeBackwards("."));
                        break;
                    }

                    case "sells": {
                        category = EventCategory.Sell;
                        item = itemParser.Parse(msg.BeforeBackwards(" each.").SkipWhitespacesBackwards());
                        break;
                    }

                    case "earns": {
                        category = EventCategory.Work;
                        amount = msg.Integer();
                        location = locationParser.Parse(msg.After(" in ").SkipWhitespaces().BeforeBackwards("."));
                        break;
                    }

                    case "produces": {
                        category = EventCategory.Produce;
                        item = itemParser.Parse(msg.Before(" in "));
                        location = locationParser.Parse(msg.After(" in ").SkipWhitespaces().BeforeBackwards("."));
                        break;
                    }

                    case "pillages": {
                        category = EventCategory.Pillage;
                        amount = Money(msg);
                        location = locationParser.Parse(msg.After(" from ").SkipWhitespaces().BeforeBackwards("."));
                        break;
                    }

                    default:
                        // other event
                        category = EventCategory.Unknown;
                        break;
                }

                List<IReportNode> keys = new () {
                    ReportNode.Str("category", category.ToString()),
                    ReportNode.Key("unit", ReportNode.Object(unit.Value)),
                    ReportNode.Str("message", message)
                };

                if (location) {
                    keys.Add(location.Value);
                }

                if (item) {
                    var objNode = item.Value as ReportObject;
                    keys.AddRange(objNode.Children);
                }

                if (amount) {
                    keys.Add(ReportNode.Int("amount", amount));
                }

                return Ok(ReportNode.Object(keys));
            }).RecoverWith<IReportNode>(() => ReportNode.Object(
                ReportNode.Str("category", EventCategory.Unknown.ToString()),
                ReportNode.Str("message", p.AsString())
            ));

            return result;
        }

        private Maybe<int> Money(TextParser p) => p.After("$").Integer();
    }
}
