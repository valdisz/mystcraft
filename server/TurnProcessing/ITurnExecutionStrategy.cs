namespace advisor.TurnProcessing;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;

public interface ITurnExecutionStrategy {
    Task RunNextAsync(CancellationToken cancellationToken = default);
}
