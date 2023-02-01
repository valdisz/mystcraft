namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;

public record GameScheduleSet(long GameId, string Schedule): IRequest<GameScheduleSetResult>;

public record GameScheduleSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameScheduleSetHandler : IRequestHandler<GameScheduleSet, GameScheduleSetResult> {
    public GameScheduleSetHandler(IGameRepository games, IUnitOfWork unit, IMediator mediator) {
        this.games = games;
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IGameRepository games;
    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GameScheduleSetResult> Handle(GameScheduleSet request, CancellationToken cancellationToken) {
        var game = await games.GetOneAsync(request.GameId);
        if (game == null) {
            return new GameScheduleSetResult(false, "Game does not exist.");
        }

        game.Options.Schedule = request.Schedule;

        await unit.SaveChangesAsync(cancellationToken);

        await mediator.Send(new Reconcile(game.Id), cancellationToken);

        return new GameScheduleSetResult(true, Game: game);
    }
}
