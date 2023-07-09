// FIXME
namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Model;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameOptionsSet(long GameId, GameOptions Options) : IRequest<GameOptionsSetResult>;
public record GameOptionsSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

// public class GameOptionsSetHandler : IRequestHandler<GameOptionsSet, GameOptionsSetResult> {
//     public GameOptionsSetHandler(IAllGamesRepository gameRepo, IMediator mediator) {
//         this.gameRepo = gameRepo;
//         this.unitOfWork = gameRepo.UnitOfWork;
//         this.mediator = mediator;
//     }

//     private readonly IAllGamesRepository gameRepo;
//     private readonly IUnitOfWork unitOfWork;
//     private readonly IMediator mediator;

//     public Task<GameOptionsSetResult> Handle(GameOptionsSet request, CancellationToken cancellationToken)
//         => unitOfWork.BeginTransaction(cancellationToken)
//             .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
//             .Validate(game => game switch {
//                 { Status: GameStatus.STOPED } => Failure<DbGame>("Game already compleated."),
//                 _ => Success(game)
//             })
//             .Do(game => game.Options = request.Options)
//             .Bind(gameRepo.Update)
//             .SaveAndReconcile(mediator, unitOfWork, cancellationToken)
//             .RunWithRollback(
//                 unitOfWork,
//                 game => new GameOptionsSetResult(true, Game: game),
//                 error => new GameOptionsSetResult(false, error.Message),
//                 cancellationToken
//             );
// }
