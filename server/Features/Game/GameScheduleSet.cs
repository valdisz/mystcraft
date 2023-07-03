namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Persistence;
using advisor.Schema;

public record GameScheduleSet(long GameId, string Schedule): IRequest<GameScheduleSetResult>;

public record GameScheduleSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameScheduleSetHandler : IRequestHandler<GameScheduleSet, GameScheduleSetResult> {
    public GameScheduleSetHandler(IAllGamesRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IAllGamesRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GameScheduleSetResult> Handle(GameScheduleSet request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
            .Validate(game => game switch {
                { Status: GameStatus.STOPED } => Failure<DbGame>("Game already compleated."),
                _ => Success(game)
            })
            .Do(game => game.Options.Schedule = request.Schedule)
            .Bind(gameRepo.Update)
            .SaveAndReconcile(mediator, unitOfWork, cancellationToken)
            .RunWithRollback(
                unitOfWork,
                game => new GameScheduleSetResult(true, Game: game),
                error => new GameScheduleSetResult(false, error.Message),
                cancellationToken
            );
}
