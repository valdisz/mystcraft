namespace advisor.Remote;

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp;

public class NewOriginsClient {
    public NewOriginsClient(string url, IHttpClientFactory httpClientFactory, IConfiguration confg = null) {
        this.url = url;
        this.httpClientFactory = httpClientFactory;
        this.context = new BrowsingContext(confg ?? Configuration.Default.WithDefaultLoader());
    }

    private readonly string url;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly BrowsingContext context;

    public async Task<int> GetCurrentTurnNumberAsync() {
        var doc = await context.OpenAsync(this.url);

        var allHeadings = doc.QuerySelectorAll("h3");
        foreach (var h in allHeadings) {
            if (h.TextContent.StartsWith("Turn Number:")) {
                var turnNumber = int.Parse(h.QuerySelector("span").TextContent);
                return turnNumber;
            }
        }

        return -1;
    }

    public async Task<Stream> DownloadReport(int factionNumber, string password) {
        using var http = httpClientFactory.CreateClient();

        var fields = new Dictionary<string, string> {
            { "factionId", factionNumber.ToString() },
            { "password", password }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{this.url}/game/download-report") {
            Content = new FormUrlEncodedContent(fields)
        };

        var response = await http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        return stream;
    }

    public async Task<NewOriginsFaction[]> ListFactionsAsync() {
        var list = new List<NewOriginsFaction>();

        var doc = await context.OpenAsync($"{this.url}/game");

        var rows = doc.QuerySelectorAll("table tbody tr");
        foreach (var row in rows) {
            var numberCol = row.QuerySelector("th");
            var numberValue = numberCol.TextContent.Trim().ToLower();

            var cols = row.QuerySelectorAll("td");

            var nameCol = cols[0];
            var ordersCol = cols[2];
            var timesCol = cols[3];

            list.Add(new NewOriginsFaction(
                Number: numberValue == "new" ? null : int.Parse(numberValue),
                Name: nameCol.TextContent.Trim(),
                OrdersSubmitted: ordersCol.QuerySelector("span") != null,
                TimesSubmitted: timesCol.QuerySelector("span") != null
            ));
        }

        return list.ToArray();
    }
}

public record NewOriginsFaction(int? Number, string Name, bool OrdersSubmitted, bool TimesSubmitted);
