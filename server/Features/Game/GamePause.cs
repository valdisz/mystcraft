namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;

public record GamePause(long GameId): IRequest<GamePauseResult>;

public record GamePauseResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GamePauseHandler : IRequestHandler<GamePause, GamePauseResult> {
    public GamePauseHandler(IGameRepository games, IUnitOfWork unit, IMediator mediator) {
        this.games = games;
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IGameRepository games;
    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GamePauseResult> Handle(GamePause request, CancellationToken cancellationToken) {
        var game = await games.PauseAsync(request.GameId, cancellationToken);
        if (game == null) {
            return new GamePauseResult(false, "Game not found.");
        }

        if (game.Status != GameStatus.PAUSED) {
            return new GamePauseResult(false, "Game cannot be paused.");
        }

        await unit.SaveChangesAsync(cancellationToken);

        await mediator.Send(new Reconcile(game.Id), cancellationToken);

        return new GamePauseResult(true, Game: game);
    }
}
