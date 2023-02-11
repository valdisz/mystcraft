namespace advisor.Features;

using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System.Threading.Tasks;
using System.Threading;

public record GamePause(long GameId): IRequest<GamePauseResult>;

public record GamePauseResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GamePauseHandler : IRequestHandler<GamePause, GamePauseResult> {
    public GamePauseHandler(IGameRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GamePauseResult> Handle(GamePause request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(_ => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.RUNNING } => Success(game),
                { Status: GameStatus.LOCKED } => Success(game),
                _ => Failure<DbGame>("Game must be in RUNNING or LOCKED state.")
            }, cancellationToken))
            .Bind(game => gameRepo.UpdateGame(game, x => x.Status = GameStatus.PAUSED)
                .Bind(() => unitOfWork.SaveChanges(cancellationToken))
                .Bind(() => Functions.Reconcile(request.GameId, mediator, cancellationToken))
                .Bind(() => unitOfWork.CommitTransaction(cancellationToken))
                .Return(game)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbGame, GamePauseResult>(
                game => new GamePauseResult(true, Game: game),
                error => new GamePauseResult(false, error.Message),
                cancellationToken
            ));
}
