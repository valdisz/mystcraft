namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameOptionsSet(long GameId, GameOptions Options) : IRequest<GameOptionsSetResult>;
public record GameOptionsSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameOptionsSetHandler : IRequestHandler<GameOptionsSet, GameOptionsSetResult> {
    public GameOptionsSetHandler(IGameRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GameOptionsSetResult> Handle(GameOptionsSet request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.COMPLEATED } => Failure<DbGame>("Game already compleated."),
                _ => Success(game)
            }, cancellationToken))
            .Bind(game => gameRepo.UpdateGame(game, x => x.Options = request.Options)
                .Bind(() => unitOfWork.SaveChanges(cancellationToken))
                .Bind(() => Functions.Reconcile(request.GameId, mediator, cancellationToken))
                .Bind(() => unitOfWork.CommitTransaction(cancellationToken))
                .Return(game)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbGame, GameOptionsSetResult>(
                game => new GameOptionsSetResult(true, Game: game),
                error => new GameOptionsSetResult(false, error.Message),
                cancellationToken
            ));
}
