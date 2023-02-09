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
    public GameNextTurnHandler(IGameRepository gameRepo, IBackgroundJobClient jobs) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.jobs = jobs;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IBackgroundJobClient jobs;

    public Task<GameNextTurnResult> Handle(GameNextTurn request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.RUNNING } => Success(game),
                _ => Failure<DbGame>("Game must be RUNNING state.")
            }, cancellationToken))
            .Bind(game => EnqueueTurnRun(jobs, request.GameId, request.TurnNumber ?? game.NextTurnNumber, request.Force)
                .Bind(jobId => unitOfWork.CommitTransaction(cancellationToken)
                    .Return(jobId)
                )
            )
            .PipeTo(unitOfWork.RunWithRollback<string, GameNextTurnResult>(
                jobId => new GameNextTurnResult(true, JobId: jobId),
                error => new GameNextTurnResult(false, error.Message),
                cancellationToken
            ));

    public static IO<string> EnqueueTurnRun(IBackgroundJobClient jobs, long gameId, int? turnNumber, GameNextTurnForceInput force)
        => () => Success(jobs.Enqueue<AllJobs>(x => x.RunTurnAsync(gameId, turnNumber, force)));
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
