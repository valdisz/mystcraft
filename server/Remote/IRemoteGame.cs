namespace advisor.Remote;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IRemoteGame {
    Task<int> GetCurrentTurnNumberAsync(CancellationToken cancellation = default);
    Task<string> DownloadReportAsync(int factionNumber, string password, CancellationToken cancellation = default);
    IAsyncEnumerable<RemoteFaction> ListFactionsAsync(CancellationToken cancellation = default);
    IAsyncEnumerable<RemoteArticle> ListArticlesAsync(CancellationToken cancellation = default);
}
