namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using Hangfire;

public record GameNextTurn(long GameId, int? TurnNumber = null): IRequest<GameNextTurnResult>;

public record GameNextTurnResult(bool IsSuccess, string Error = null, string JobId = null) : MutationResult(IsSuccess, Error);

public class GameNextTurnHandler : IRequestHandler<GameNextTurn, GameNextTurnResult> {
    public GameNextTurnHandler(IUnitOfWork unit, IBackgroundJobClient jobs) {
        this.unit = unit;
        this.jobs = jobs;
    }

    private readonly IUnitOfWork unit;
    private readonly IBackgroundJobClient jobs;

    public async Task<GameNextTurnResult> Handle(GameNextTurn request, CancellationToken cancellationToken) {
        var gamesRepo = unit.Games;

        var game = await gamesRepo.StartAsync(request.GameId, cancellationToken);
        if (game == null) {
            return new GameNextTurnResult(false, "Game not found.");
        }

        if (game.Status != GameStatus.RUNNING) {
            return new GameNextTurnResult(false, "Game must be in runnung state.");
        }

        var gameId = request.GameId;
        var turnNumber = request.TurnNumber ?? game.NextTurnNumber;

        var jobId = jobs.Enqueue<GameNextTurnJob>(x => x.RunAsync(gameId, turnNumber));

        return new GameNextTurnResult(true, JobId: jobId);
    }
}

[System.Serializable]
public class GameNextTurnException : System.Exception
{
    public GameNextTurnException() { }
    public GameNextTurnException(string message) : base(message) { }
    public GameNextTurnException(string message, System.Exception inner) : base(message, inner) { }
    protected GameNextTurnException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
