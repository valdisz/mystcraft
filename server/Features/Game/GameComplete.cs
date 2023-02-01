namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;

public record GameComplete(long GameId): IRequest<GameCompleteResult>;

public record GameCompleteResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameCompleteHandler : IRequestHandler<GameComplete, GameCompleteResult> {
    public GameCompleteHandler(IGameRepository games, IUnitOfWork unit, IMediator mediator) {
        this.games = games;
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IGameRepository games;
    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GameCompleteResult> Handle(GameComplete request, CancellationToken cancellationToken) {
        await unit.BeginTransactionAsync(cancellationToken);

        var game = await games.GetOneAsync(request.GameId, cancellation: cancellationToken);
        if (game == null) {
            return new GameCompleteResult(false, "Game not found.");
        }

        game.Status = GameStatus.COMPLEATED;
        await unit.SaveChangesAsync(cancellationToken);

        await mediator.Send(new Reconcile(game.Id), cancellationToken);
        await unit.CommitTransactionAsync(cancellationToken);

        return new GameCompleteResult(true, Game: game);
    }
}
