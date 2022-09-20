namespace advisor.Features;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using MediatR;

public record GameCreateRemote(string Name, GameOptions Options) : IRequest<GameCreateRemoteResult>;

public record GameCreateRemoteResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

public class GameCreateRemoteHandler : IRequestHandler<GameCreateRemote, GameCreateRemoteResult> {
    public GameCreateRemoteHandler(IUnitOfWork unit, IMediator mediator, IHttpClientFactory httpFactory) {
        this.unit = unit;
        this.mediator = mediator;
        this.games = unit.Games;
        this.httpFactory = httpFactory;
    }

    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;
    private readonly IGameRepository games;
    private readonly IHttpClientFactory httpFactory;

    public async Task<GameCreateRemoteResult> Handle(GameCreateRemote request, CancellationToken cancellationToken) {
        var remote = new NewOriginsClient(request.Options.ServerAddress, httpFactory);

        var turnNumber = await remote.GetCurrentTurnNumberAsync(cancellationToken);
        if (turnNumber < 0) {
            return new GameCreateRemoteResult(false, "Cannot get remote turn number.");
        }

        await unit.BeginTransactionAsync(cancellationToken);

        var game = await games.CreateRemoteAsync(
            name: request.Name,
            serverAddress: request.Options.ServerAddress,
            turnNumber: turnNumber,
            options: request.Options,
            cancellationToken
        );

        await unit.SaveChangesAsync(cancellationToken);

        await mediator.Send(new GameSyncFactions(game.Id), cancellationToken);
        // TODO: import articles

        await unit.CommitTransactionAsync(cancellationToken);

        await mediator.Send(new JobReconcile(game.Id));

        return new GameCreateRemoteResult(true, Game: game);
    }
}
