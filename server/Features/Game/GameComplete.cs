namespace advisor.Features;

using MediatR;
using advisor.Schema;
using advisor.Persistence;
using System.Threading.Tasks;
using System.Threading;

public record GameComplete(long GameId): IRequest<GameCompleteResult>;

public record GameCompleteResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameCompleteHandler : IRequestHandler<GameComplete, GameCompleteResult> {
    public GameCompleteHandler(IGameRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GameCompleteResult> Handle(GameComplete request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
            .Validate(game => game switch {
                { Status: GameStatus.COMPLEATED } => Failure<DbGame>("Game already compleated."),
                _ => Success(game)
            })
            .Do(game => game.Status = GameStatus.COMPLEATED)
            .Bind(gameRepo.Update)
            .SaveAndReconcile(mediator, unitOfWork, cancellationToken)
            .RunWithRollback(
                unitOfWork,
                game => new GameCompleteResult(true, Game: game),
                error => new GameCompleteResult(false, error.Message),
                cancellationToken
            );
}
