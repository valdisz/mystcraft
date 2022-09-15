namespace advisor.Remote;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
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

    public async Task<int> GetCurrentTurnNumberAsync(CancellationToken cancellation) {
        var doc = await context.OpenAsync(this.url, cancellation);

        var allHeadings = doc.QuerySelectorAll("h3");
        foreach (var h in allHeadings) {
            if (h.TextContent.StartsWith("Turn Number:")) {
                var turnNumber = int.Parse(h.QuerySelector("span").TextContent);
                return turnNumber + 1;  // because New Origins all turns starts from 0, bet we start from 1
            }
        }

        return -1;
    }

    public async Task<string> DownloadReportAsync(int factionNumber, string password, CancellationToken cancellation) {
        using var http = httpClientFactory.CreateClient();

        var fields = new Dictionary<string, string> {
            { "factionId", factionNumber.ToString() },
            { "password", password }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{this.url}/game/download-report") {
            Content = new FormUrlEncodedContent(fields)
        };

        var response = await http.SendAsync(request, cancellation);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellation);
        using var reader = new StreamReader(stream);

        var contents = await reader.ReadToEndAsync();

        if (contents.StartsWith("<!doctype html>")) {
            throw new WrongFactionOrPasswordException();
        }

        return contents;
    }

    public async IAsyncEnumerable<NewOriginsFaction> ListFactionsAsync([EnumeratorCancellation] CancellationToken cancellation) {
        var doc = await context.OpenAsync($"{this.url}/game", cancellation);

        var rows = doc.QuerySelectorAll("table tbody tr");
        foreach (var row in rows) {
            var numberCol = row.QuerySelector("th");
            var numberValue = numberCol.TextContent.Trim().ToLower();

            var cols = row.QuerySelectorAll("td");

            var nameCol = cols[0];
            var ordersCol = cols[2];
            var timesCol = cols[3];

            var faction = new NewOriginsFaction(
                Number: numberValue == "new" ? null : int.Parse(numberValue),
                Name: nameCol.TextContent.Trim(),
                OrdersSubmitted: ordersCol.QuerySelector("span") != null,
                TimesSubmitted: timesCol.QuerySelector("span") != null
            );

            yield return faction;
        }
    }

    public async IAsyncEnumerable<NewOriginsArticle> ListArticlesAsync([EnumeratorCancellation] CancellationToken cancellation) {
        var doc = await context.OpenAsync($"{this.url}/times", cancellation);

        var root = doc.QuerySelector(".container.my-5");
        foreach (var child in root.Children) {

        }

        yield break;
    }
}

[System.Serializable]
public class WrongFactionOrPasswordException : System.Exception
{
    public WrongFactionOrPasswordException() { }
    public WrongFactionOrPasswordException(string message) : base(message) { }
    public WrongFactionOrPasswordException(string message, System.Exception inner) : base(message, inner) { }
    protected WrongFactionOrPasswordException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public record NewOriginsFaction(int? Number, string Name, bool OrdersSubmitted, bool TimesSubmitted);
public record NewOriginsArticle(string Name, string Contents);
