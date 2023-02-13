namespace advisor.Features;

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record GameRulesetSet(long GameId, Stream Ruleset) : IRequest<GameRulesetSetResult>;
public record GameRulesetSetResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameRulesetSetHandler : IRequestHandler<GameRulesetSet, GameRulesetSetResult> {
    public GameRulesetSetHandler(IGameRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GameRulesetSetResult> Handle(GameRulesetSet request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.COMPLEATED } => Failure<DbGame>("Game already compleated."),
                _ => Success(game)
            }, cancellationToken))
            .Bind(game => ReadStream(request.Ruleset, cancellationToken)
                .Bind(ruleset => gameRepo.UpdateGame(game, x => x.Ruleset = ruleset))
                .Bind(() => unitOfWork.SaveChanges(cancellationToken))
                .Bind(() => Functions.Reconcile(request.GameId, mediator, cancellationToken))
                .Return(game)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbGame, GameRulesetSetResult>(
                game => new GameRulesetSetResult(true, Game: game),
                error => new GameRulesetSetResult(false, error.Message),
                cancellationToken
            ));

    public static AsyncIO<byte[]> ReadStream(Stream stream, CancellationToken cancellation)
        => AsyncEffect(() => stream.ReadAllBytesAsync(cancellation));
}
