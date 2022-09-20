namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameOptionsSet(long GameId, GameOptions Options) : IRequest<GameOptionsSetResult>;
public record GameOptionsSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

public class GameOptionsSetHandler : IRequestHandler<GameOptionsSet, GameOptionsSetResult> {
    public GameOptionsSetHandler(IUnitOfWork unit, IMediator mediator) {
        this.unit = unit;
        this.mediator = mediator;
    }

    private readonly IUnitOfWork unit;
    private readonly IMediator mediator;

    public async Task<GameOptionsSetResult> Handle(GameOptionsSet request, CancellationToken cancellationToken) {
        var game = await unit.Games.GetOneAsync(request.GameId);
        if (game == null) {
            return new GameOptionsSetResult(false, "Game does not exist.");
        }

        game.Options = request.Options;

        await unit.SaveChangesAsync(cancellationToken);

        await mediator.Send(new JobReconcile(game.Id));

        return new GameOptionsSetResult(true, Game: game);
    }
}
