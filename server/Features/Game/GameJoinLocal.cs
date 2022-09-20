namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameJoinLocal(long UserId, long GameId, string Name) : IRequest<GameJoinLocalResult>;

public record GameJoinLocalResult(bool IsSuccess, string Error = null, DbRegistration Registration = null) : IMutationResult;

public class GameJoinLocalHandler : IRequestHandler<GameJoinLocal, GameJoinLocalResult> {
    public GameJoinLocalHandler(IUnitOfWork unit) {
        this.unit = unit;
        this.gamesRepo = unit.Games;
    }

    private readonly IUnitOfWork unit;
    private readonly IGameRepository gamesRepo;

    public async Task<GameJoinLocalResult> Handle(GameJoinLocal request, CancellationToken cancellationToken) {
        var game = await gamesRepo.GetOneNoTrackingAsync(request.GameId);
        if (game == null) {
            return new GameJoinLocalResult(false, "Game does not exist.");
        }

        DbRegistration reg;
        try {
            reg = await gamesRepo.RegisterAsync(request.GameId, request.UserId, request.Name, cancellationToken);
        }
        catch (RepositoryException ex) {
            return new GameJoinLocalResult(false, ex.Message);
        }

        await unit.SaveChangesAsync(cancellationToken);

        return new GameJoinLocalResult(true, Registration: reg);
    }
}
