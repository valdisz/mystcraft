namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using Hangfire;
using System;

public record GameNextTurn(long GameId, int? TurnNumber = null, GameNextTurnForceInput Force = null): IRequest<GameNextTurnResult>;

public record GameNextTurnResult(bool IsSuccess, string Error = null, string JobId = null) : MutationResult(IsSuccess, Error);

public class GameNextTurnForceInput {
    public bool Parse { get; set; }
    public bool Process { get; set; }
    public bool Merge { get; set; }
}

public class GameNextTurnHandler : IRequestHandler<GameNextTurn, GameNextTurnResult> {
    public GameNextTurnHandler(IGameRepository games, IUnitOfWork unit, IBackgroundJobClient jobs) {
        this.games = games;
        this.unit = unit;
        this.jobs = jobs;
    }

    private readonly IGameRepository games;
    private readonly IUnitOfWork unit;
    private readonly IBackgroundJobClient jobs;

    public async Task<GameNextTurnResult> Handle(GameNextTurn request, CancellationToken cancellationToken) {
        await unit.BeginTransactionAsync(cancellationToken);

        var game = await games.GetOneAsync(request.GameId, cancellation: cancellationToken);
        if (game == null) {
            return new GameNextTurnResult(false, "Game not found.");
        }

        if (request.TurnNumber == null) {
            if (game.Status != GameStatus.RUNNING) {
                return new GameNextTurnResult(false, "Game must be in runnung state.");
            }
        }
        else {
            if (game.Status != GameStatus.RUNNING && game.Status != GameStatus.LOCKED) {
                return new GameNextTurnResult(false, "Game must be in runnung or locked state.");
            }
        }

        game.Status = GameStatus.RUNNING;

        var gameId = request.GameId;
        var turnNumber = request.TurnNumber ?? game.NextTurnNumber;

        await unit.CommitTransactionAsync(cancellationToken);

        try {
            var jobId = jobs.Enqueue<AllJobs>(x => x.RunTurnAsync(gameId, turnNumber, request.Force));
            return new GameNextTurnResult(true, JobId: jobId);
        }
        catch (Exception ex) {
            return new GameNextTurnResult(false, ex.ToString());
        }
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