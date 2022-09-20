namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;

public record GameComplete(long GameId): IRequest<GameCompleteResult>;

public record GameCompleteResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

public class GameCompleteHandler : IRequestHandler<GameComplete, GameCompleteResult> {
    public GameCompleteHandler(IUnitOfWork unit, IMediator mediator) {
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GameCompleteResult> Handle(GameComplete request, CancellationToken cancellationToken) {
        var gamesRepo = unit.Games;

        var game = await gamesRepo.PauseAsync(request.GameId, cancellationToken);
        if (game == null) {
            return new GameCompleteResult(false, "Game not found.");
        }

        if (game.Status != GameStatus.COMPLEATED) {
            return new GameCompleteResult(false, "Game cannot be completed.");
        }

        await unit.SaveChangesAsync(cancellationToken);

        await mediator.Send(new JobReconcile(game.Id), cancellationToken);

        return new GameCompleteResult(true, Game: game);
    }
}
