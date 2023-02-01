namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameJoinLocal(long UserId, long GameId, string Name) : IRequest<GameJoinLocalResult>;

public record GameJoinLocalResult(bool IsSuccess, string Error = null, DbRegistration Registration = null) : MutationResult(IsSuccess, Error);

public class GameJoinLocalHandler : IRequestHandler<GameJoinLocal, GameJoinLocalResult> {
    public GameJoinLocalHandler(IGameRepository games, IUnitOfWork unit) {
        this.unit = unit;
        this.games = games;
    }

    private readonly IUnitOfWork unit;
    private readonly IGameRepository games;

    public async Task<GameJoinLocalResult> Handle(GameJoinLocal request, CancellationToken cancellationToken) {
        var game = await games.GetOneAsync(request.GameId, withTracking: false);

        switch (game?.Status) {
            case null: return new GameJoinLocalResult(false, "Game does not exist.");
            case GameStatus.PAUSED: return new GameJoinLocalResult(false, "Game paused.");
            case GameStatus.LOCKED: return new GameJoinLocalResult(false, "Game is processing turn.");
            case GameStatus.COMPLEATED: return new GameJoinLocalResult(false, "Game compleated.");
        }

        DbRegistration reg;
        try {
            reg = await games.RegisterAsync(request.GameId, request.UserId, request.Name, cancellationToken);
        }
        catch (RepositoryException ex) {
            return new GameJoinLocalResult(false, ex.Message);
        }

        await unit.SaveChangesAsync(cancellationToken);

        return new GameJoinLocalResult(true, Registration: reg);
    }
}
