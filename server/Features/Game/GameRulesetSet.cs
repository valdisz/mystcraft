namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameRulesetSet(long GameId, string Ruleset) : IRequest<GameRulesetSetResult>;
public record GameRulesetSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameRulesetSetHandler : IRequestHandler<GameRulesetSet, GameRulesetSetResult> {
    public GameRulesetSetHandler(IGameRepository games, IUnitOfWork unit) {
        this.games = games;
        this.unit = unit;
    }

    private readonly IGameRepository games;
    private readonly IUnitOfWork unit;

    public async Task<GameRulesetSetResult> Handle(GameRulesetSet request, CancellationToken cancellationToken) {
        var game = await games.GetOneAsync(request.GameId);
        if (game == null) {
            return new GameRulesetSetResult(false, "Game does not exist.");
        }

        game.Ruleset = request.Ruleset;

        await unit.SaveChangesAsync(cancellationToken);

        return new GameRulesetSetResult(true, Game: game);
    }
}
