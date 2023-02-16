namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;

public record GameStart(long GameId): IRequest<GameStartResult>;

public record GameStartResult(bool IsSuccess, string Error = null, DbGame Game = null, string JobId = null) : MutationResult(IsSuccess, Error);

public class GameStartHandler : IRequestHandler<GameStart, GameStartResult> {
    public GameStartHandler(IGameRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GameStartResult> Handle(GameStart request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
            .Validate(game => game switch {
                { Status: GameStatus.NEW } => Success(game),
                { Status: GameStatus.PAUSED } => Success(game),
                _ => Failure<DbGame>("Game must be in NEW or PAUSED state.")
            })
            .Do(game => game.Status = GameStatus.RUNNING)
            .Bind(gameRepo.Update)
            .SaveAndReconcile(mediator, unitOfWork, cancellationToken)
            .RunWithRollback(
                unitOfWork,
                game => new GameStartResult(true, Game: game),
                error => new GameStartResult(false, error.Message),
                cancellationToken
            );
}
