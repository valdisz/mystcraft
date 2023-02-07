namespace advisor.Features;

using System;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GameJoinLocal(long UserId, long GameId, string Name) : IRequest<GameJoinLocalResult>;

public record GameJoinLocalResult(bool IsSuccess, string Error = null, DbRegistration Registration = null) : MutationResult(IsSuccess, Error);

public class GameJoinLocalHandler : IRequestHandler<GameJoinLocal, GameJoinLocalResult> {
    public GameJoinLocalHandler(IGameRepository gameRepo) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;

    public Task<GameJoinLocalResult> Handle(GameJoinLocal request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(_ => gameRepo.GetOneGame(request.GameId, withTracking: true, cancellation: cancellationToken))
            .Bind<Option<DbGame>, DbGame>(maybeGame => () => maybeGame
                .Select(game => game switch {
                    { Type: Persistence.GameType.REMOTE } => Failure<DbGame>("Cannot join remote game."),
                    { Status: GameStatus.NEW } => Failure<DbGame>("Game not yet started."),
                    { Status: GameStatus.LOCKED } => Failure<DbGame>("Game is processing a turn."),
                    { Status: GameStatus.COMPLEATED }  => Failure<DbGame>("Game compleated."),
                    _ => Success(game)
                })
                .Unwrap(() => Failure<DbGame>("Game does not exist."))
            )
            .Bind(game => DoesNotHaveRegistration(game, request.UserId, cancellationToken)
                .Bind(_ => DoesNotHaveActivePlayer(game, request.UserId, cancellationToken))
                .Select(_ => gameRepo.Add(game, DbRegistration.Create(request.UserId, request.Name, Guid.NewGuid().ToString("N"))))
            )
            .Bind(reg => unitOfWork.CommitTransaction(cancellationToken)
                .Select(_ => new GameJoinLocalResult(true, Registration: reg))
            )
            .Run()
            .OnFailure(_ => unitOfWork.RollbackTransaction(cancellationToken).Run())
            .Unwrap(error => new GameJoinLocalResult(false, error.Message));

    private AsyncIO<advisor.Unit> DoesNotHaveRegistration(DbGame game, long userId, CancellationToken cancellation)
        => async () => await gameRepo.QueryRegistrations(game)
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId, cancellation)
                ? Failure<advisor.Unit>("Game already includes registration from this user.")
                : Success(unit);

    private AsyncIO<advisor.Unit> DoesNotHaveActivePlayer(DbGame game, long userId, CancellationToken cancellation)
        =>  async () => await gameRepo.QueryPlayers(game)
            .AsNoTracking()
            .OnlyActivePlayers()
            .AnyAsync(x => x.UserId == userId, cancellation)
                ? Failure<advisor.Unit>("Game already includes active player from this user.")
                : Success(unit);
}
