namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;

public record GameStart(long GameId): IRequest<GameStartResult>;

public record GameStartResult(bool IsSuccess, string Error = null, DbGame Game = null, string JobId = null) : MutationResult(IsSuccess, Error);

public class GameStartHandler : IRequestHandler<GameStart, GameStartResult> {
    public GameStartHandler(IGameRepository games, IUnitOfWork unit, IMediator mediator) {
        this.games = games;
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IGameRepository games;
    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GameStartResult> Handle(GameStart request, CancellationToken cancellationToken) {
        await unit.BeginTransactionAsync(cancellationToken);

        var game = await games.StartAsync(request.GameId, cancellationToken);
        if (game == null) {
            return new GameStartResult(false, "Game not found.");
        }

        if (game.Status != GameStatus.RUNNING) {
            return new GameStartResult(false, "Game cannot be started.");
        }

        string jobId = null;
        if (game.LastTurnNumber == null && game.Type == Persistence.GameType.REMOTE) {
            var result = await mediator.Send(new GameNextTurn(game.Id), cancellationToken);
            if (!result.IsSuccess) {
                return new GameStartResult(result.IsSuccess, result.Error);
            }

            jobId = result.JobId;
        }

        if (!await unit.CommitTransactionAsync(cancellationToken)) {
            return new GameStartResult(false);
        }

        await mediator.Send(new Reconcile(game.Id), cancellationToken);

        return new GameStartResult(true, Game: game, JobId: jobId);
    }
}
