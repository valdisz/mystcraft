namespace advisor.Features;

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using MediatR;

public record GameCreateRemote(string Name, GameOptions Options) : IRequest<GameCreateRemoteResult>;

public record GameCreateRemoteResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

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
        await unit.BeginTransactionAsync(cancellationToken);

        var game = await games.CreateRemoteAsync(
            name: request.Name,
            serverAddress: request.Options.ServerAddress,
            options: request.Options,
            cancellationToken
        );

        await unit.SaveChangesAsync(cancellationToken);

        var syncResult = await mediator.Send(new GameSyncFactions(game.Id), cancellationToken);
        if (!syncResult) {
            return new GameCreateRemoteResult(syncResult.IsSuccess, syncResult.Error);
        }

        if (await unit.CommitTransactionAsync(cancellationToken)) {
            await mediator.Send(new Reconcile(game.Id), cancellationToken);
        }

        return new GameCreateRemoteResult(true, Game: game);
    }
}
