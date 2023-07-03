// FIXME
namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using Hangfire;
using System;

public record GameNextTurn(long GameId, int? TurnNumber, GameNextTurnForceInput Force = null): IRequest<GameNextTurnResult>;

public record GameNextTurnResult(bool IsSuccess, string Error = null, DbGame Game = null, string JobId = null) : MutationResult(IsSuccess, Error);

public class GameNextTurnForceInput {
    public bool Parse { get; set; }
    public bool Process { get; set; }
    public bool Merge { get; set; }
}

// public class GameNextTurnHandler : IRequestHandler<GameNextTurn, GameNextTurnResult> {
//     public GameNextTurnHandler(IAllGamesRepository gameRepo, IBackgroundJobClient jobs) {
//         this.gameRepo = gameRepo;
//         this.unitOfWork = gameRepo.UnitOfWork;
//         this.jobs = jobs;
//     }

//     private readonly IAllGamesRepository gameRepo;
//     private readonly IUnitOfWork unitOfWork;
//     private readonly IBackgroundJobClient jobs;

//     private record struct GameAndJob(DbGame Game, string JobId);

//     public Task<GameNextTurnResult> Handle(GameNextTurn request, CancellationToken cancellationToken)
//         => unitOfWork.BeginTransaction(cancellationToken)
//             .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
//             .Validate(game => game switch {
//                 { Status: GameStatus.RUNNING } => Success(game),
//                 _ => Failure<DbGame>("Game must be in RUNNING state")
//             })
//             .Do(game => game.Status = GameStatus.LOCKED)
//             .Do(game => gameRepo.Update(game))
//             .Bind(game => unitOfWork.SaveChanges(cancellationToken)
//                 .Return(game)
//             )
//             .Bind(game => EnqueueTurnRun(jobs, request.GameId, request.Force)
//                 .Select(jobId => new GameAndJob(game, jobId))
//             )
//             .RunWithRollback(
//                 unitOfWork,
//                 x => new GameNextTurnResult(true, Game: x.Game, JobId: x.JobId),
//                 error => new GameNextTurnResult(false, error.Message),
//                 cancellationToken
//             );

//     public static IO<string> EnqueueTurnRun(IBackgroundJobClient jobs, long gameId, GameNextTurnForceInput force)
//         => () => Success(jobs.Enqueue<AllJobs>(x => x.RunTurnAsync(gameId, force, CancellationToken.None)));
// }

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
