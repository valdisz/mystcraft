namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;

public record GameStart(long GameId): IRequest<GameStartResult>;

public record GameStartResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

public class GameStartHandler : IRequestHandler<GameStart, GameStartResult> {
    public GameStartHandler(IUnitOfWork unit, IMediator mediator) {
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GameStartResult> Handle(GameStart request, CancellationToken cancellationToken) {
        var gamesRepo = unit.Games;

        var game = await gamesRepo.StartAsync(request.GameId, cancellationToken);
        if (game == null) {
            return new GameStartResult(false, "Game not found.");
        }

        await unit.BeginTransactionAsync(cancellationToken);

        if (game.Status != GameStatus.RUNNING) {
            return new GameStartResult(false, "Game cannot be started.");
        }
        else {
            if (game.LastTurnNumber == null && game.Type == Persistence.GameType.REMOTE) {
                await mediator.Send(new GameTurnRun(game.Id), cancellationToken);
            }
        }

        await unit.SaveChangesAsync(cancellationToken);

        if (await unit.CommitTransactionAsync(cancellationToken)) {
            await mediator.Send(new JobReconcile(game.Id), cancellationToken);
        }

        return new GameStartResult(true, Game: game);
    }
}
