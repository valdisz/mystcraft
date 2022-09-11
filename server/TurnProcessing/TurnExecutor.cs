namespace advisor.TurnProcessing;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;

public class TurnExecutor {
    public TurnExecutor(Database db, ITurnExecutionStrategy strategy) {

    }

    public Task RunAsync(long gameId, int turnNumber, CancellationToken cancellationToken = default) {
        throw new System.NotImplementedException();
    }

    public Task RunNextAsync(long gameId, CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}
