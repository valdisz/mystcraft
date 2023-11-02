// FIXME
namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;
using Hangfire;
using System;
using advisor.Model;
using System.Linq;

public record GameNextTurn(long GameId, int? TurnNumber, GameNextTurnForceInput Force = null): IRequest<GameNextTurnResult>;

public record GameNextTurnResult(bool IsSuccess, string Error = null, DbGame Game = null, string JobId = null) : MutationResult(IsSuccess, Error);

public class GameNextTurnForceInput {
    public bool Parse { get; set; }
    public bool Process { get; set; }
    public bool Merge { get; set; }
}

public class GameNextTurnHandler : IRequestHandler<GameNextTurn, GameNextTurnResult> {
    public Task<GameNextTurnResult> Handle(GameNextTurn request, CancellationToken cancellationToken) {
        throw new NotImplementedException();
    }

    private static Mystcraft<(PlayersOutStream playersOut, GameOutStream gameOut, Seq<ReportStream> reports, Seq<MessageStream> messages)> executeEngine(GameEngineStream engine, PlayersInStream playersIn, GameInStream gameIn, Seq<FactionOrders> orders) =>
        from folder in Mystcraft.OpenWorkFolder()
        from input in Mystcraft.WriteGameState(folder, engine, playersIn, gameIn, orders)
        // TODO: make timeout configurable
        from result in Mystcraft.RunEngine(input, TimeSpan.FromMinutes(10))
        let output = result.WorkFolder
        from gameOut in Mystcraft.ReadGame(output)
        from playersOut in Mystcraft.ReadPlayers(output)
        from reports in Mystcraft.ReadReports(output)
        from messages in Mystcraft.ReadMessages(output)
        select (playersOut, gameOut, reports, messages);

    private static Mystcraft<LanguageExt.Unit> RunTurn(GameId gameId, TurnNumber turnNumber) =>
        from game in Mystcraft.ReadOneGame(gameId)

        from engine in Mystcraft.ReadOneGameEngine(new GameEngineId(game.Value.EngineId.Value))
        let engineStream = GameEngineStream.New(engine.Value.Engine)

        from turn in Mystcraft.WriteOneTurn(gameId, turnNumber)
        let playersIn = PlayersInStream.New(turn.PlayerData)
        let gameIn = GameInStream.New(turn.GameData)

        from orders in Mystcraft.ReadTurnOrders(gameId, turnNumber)

        from result in executeEngine(engineStream, playersIn, gameIn, orders)

        let nextTurnNumber = TurnNumber.New(turnNumber.Value + 1)
        // from nextTurn in Mystcraft.CreateTurn(gameId, nextTurnNumber, result.playersOut, result.gameOut)
        // read reports
        // read messages
        // save it all
        // schedule processing
        select unit;
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
