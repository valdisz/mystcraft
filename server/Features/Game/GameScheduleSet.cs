namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;

public record GameScheduleSet(long GameId, string Schedule): IRequest<GameScheduleSetResult>;

public record GameScheduleSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameScheduleSetHandler : IRequestHandler<GameScheduleSet, GameScheduleSetResult> {
    public GameScheduleSetHandler(IGameRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GameScheduleSetResult> Handle(GameScheduleSet request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.COMPLEATED } => Failure<DbGame>("Game already compleated."),
                _ => Success(game)
            }, cancellationToken))
            .Bind(game => gameRepo.UpdateGame(game, x => x.Options.Schedule = request.Schedule)
                .Bind(() => unitOfWork.SaveChanges(cancellationToken))
                .Bind(() => Functions.Reconcile(request.GameId, mediator, cancellationToken))
                .Return(game)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbGame, GameScheduleSetResult>(
                game => new GameScheduleSetResult(true, Game: game),
                error => new GameScheduleSetResult(false, error.Message),
                cancellationToken
            ));
}
