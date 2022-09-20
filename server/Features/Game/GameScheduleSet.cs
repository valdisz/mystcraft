namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;

public record GameScheduleSet(long GameId, string Schedule): IRequest<GameScheduleSetResult>;

public record GameScheduleSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

public class GameScheduleSetHandler : IRequestHandler<GameScheduleSet, GameScheduleSetResult> {
    public GameScheduleSetHandler(IUnitOfWork unit, IMediator mediator) {
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GameScheduleSetResult> Handle(GameScheduleSet request, CancellationToken cancellationToken) {
        var game = await unit.Games.GetOneAsync(request.GameId);
        if (game == null) {
            return new GameScheduleSetResult(false, "Game does not exist.");
        }

        game.Options.Schedule = request.Schedule;

        await unit.SaveChangesAsync(cancellationToken);

        await mediator.Send(new JobReconcile(game.Id));

        return new GameScheduleSetResult(true, Game: game);
    }
}
