namespace advisor
{
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using System.Text;

    public class OrdersSection : IReportSectionParser {
        public Task<bool> CanParseAsync(Cursor<TextParser> cursor) {
            var result = cursor.Value.Match("Orders Template (Long Format):");
            return Task.FromResult(result.Success);
        }

        public async Task ParseAsync(Cursor<TextParser> cursor, JsonWriter writer) {
            await cursor.SkipEmptyLines();
            var (number, password) = ParseOrdersHeaderLine(cursor.Value);

            await writer.WritePropertyNameAsync("ordersTemplate");
            await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("faction");
                await writer.WriteValueAsync(number);

                await writer.WritePropertyNameAsync("password");
                await writer.WriteValueAsync(password);

                await writer.WritePropertyNameAsync("units");
                await writer.WriteStartArrayAsync();

                while (await cursor.SkipUntil(c => Task.FromResult<bool>(c.Value.Match("unit")))) {
                    var p = cursor.Value;
                    var unitNumber = p.SkipWhitespaces().Integer();

                    StringBuilder orders = new StringBuilder();
                    while (await cursor.NextAsync()) {
                        if (cursor.Value.Match(";")) continue;

                        if (cursor.Value.Match("unit")) {
                            cursor.Value.Reset();
                            cursor.Back();
                            break;
                        }

                        if (cursor.Value.Match("#end")) break;

                        orders.AppendLine(cursor.Value.AsString());
                    }

                    await writer.WriteStartObjectAsync();
                        await writer.WritePropertyNameAsync("unit");
                        await writer.WriteValueAsync(unitNumber.Value);

                        await writer.WritePropertyNameAsync("orders");
                        await writer.WriteValueAsync(orders.ToString().Trim().Replace("\r\n", "\n"));
                    await writer.WriteEndObjectAsync();
                }

                await writer.WriteEndArrayAsync();
            await writer.WriteEndObjectAsync();
        }

        private (int number, string password) ParseOrdersHeaderLine(TextParser p) {
            var number = p.After("#atlantis").SkipWhitespaces().Integer();
            var password = p.After("\"").BeforeBackwards("\"").AsString();

            return (number, password);
        }
    }
}
