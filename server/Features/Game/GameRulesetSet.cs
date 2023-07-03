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
    public GameRulesetSetHandler(IAllGamesRepository gameRepo, IMediator mediator) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.mediator = mediator;
    }

    private readonly IAllGamesRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IMediator mediator;

    public Task<GameRulesetSetResult> Handle(GameRulesetSet request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
            .Validate(game => game switch {
                { Status: GameStatus.STOPED } => Failure<DbGame>("Game already compleated."),
                _ => Success(game)
            })
            .Bind(game => ReadStream(request.Ruleset, cancellationToken)
                .Do(ruleset => game.Ruleset = ruleset)
                .Return(game)
            )
            .Bind(gameRepo.Update)
            .SaveAndReconcile(mediator, unitOfWork, cancellationToken)
            .RunWithRollback(
                unitOfWork,
                game => new GameRulesetSetResult(true, Game: game),
                error => new GameRulesetSetResult(false, error.Message),
                cancellationToken
            );

    public static AsyncIO<byte[]> ReadStream(Stream stream, CancellationToken cancellation)
        => AsyncEffect(() => stream.ReadAllBytesAsync(cancellation));
}
