namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameJoinLocal(long UserId, long GameId, string Name) : IRequest<GameJoinLocalResult>;

public record GameJoinLocalResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

public class GameJoinLocalHandler : IRequestHandler<GameJoinLocal, GameJoinLocalResult> {
    public GameJoinLocalHandler(IUnitOfWork unit) {
        this.unit = unit;
        this.games = unit.Games;
    }

    private readonly IUnitOfWork unit;
    private readonly IGameRepository games;

    public async Task<GameJoinLocalResult> Handle(GameJoinLocal request, CancellationToken cancellationToken) {
        var game = await games.GetOneNoTrackingAsync(request.GameId);
        if (game == null) {
            return new GameJoinLocalResult(false, "Game does not exist.");
        }

        var players = unit.Players(game);

        DbPlayer player;
        try {
            player = await players.AddLocalAsync(request.Name, request.UserId, cancellationToken);
        }
        catch (PlayersRepositoryException ex) {
            return new GameJoinLocalResult(false, ex.Message);
        }

        await unit.SaveChangesAsync(cancellationToken);

        return new GameJoinLocalResult(true, Player: player);
    }
}
