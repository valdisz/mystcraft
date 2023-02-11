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
    public GameJoinLocalHandler(IGameRepository gameRepo, IPlayerRepository playerRepo) {
        this.gameRepo = gameRepo;
        this.playerRepo = playerRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
    }

    private readonly IGameRepository gameRepo;
    private readonly IPlayerRepository playerRepo;
    private readonly IUnitOfWork unitOfWork;

    public Task<GameJoinLocalResult> Handle(GameJoinLocal request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(_ => gameRepo.GetOneGame(request.GameId, withTracking: true, cancellation: cancellationToken))
            .Select(maybeGame => maybeGame
                .Select(game => game switch {
                    { Type: Persistence.GameType.REMOTE } => Failure<DbGame>("Cannot join remote game."),
                    { Status: GameStatus.NEW } => Failure<DbGame>("Game not yet started."),
                    { Status: GameStatus.LOCKED } => Failure<DbGame>("Game is processing a turn."),
                    { Status: GameStatus.COMPLEATED }  => Failure<DbGame>("Game compleated."),
                    _ => Success(game)
                })
                .Unwrap(() => Failure<DbGame>("Game does not exist."))
            )
            .Select(game => playerRepo.Specialize(game))
            .Bind(repo => DoesNotHaveRegistration(repo, request.UserId, cancellationToken)
                .Bind(_ => DoesNotHaveActivePlayer(repo, request.UserId, cancellationToken))
                .Select(_ => repo.Add(DbRegistration.Create(request.UserId, request.Name, Guid.NewGuid().ToString("N"))))
            )
            .Bind(reg => unitOfWork.CommitTransaction(cancellationToken).Return(reg))
            .PipeTo(unitOfWork.RunWithRollback<DbRegistration, GameJoinLocalResult>(
                reg => new GameJoinLocalResult(true, Registration: reg),
                error => new GameJoinLocalResult(false, error.Message),
                cancellationToken
            ));

    private AsyncIO<advisor.Unit> DoesNotHaveRegistration(ISpecializedPlayerRepository repo, long userId, CancellationToken cancellation)
        => async () => await repo.Registrations
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId, cancellation)
                ? Failure<advisor.Unit>("Game already includes registration from this user.")
                : Success(unit);

    private AsyncIO<advisor.Unit> DoesNotHaveActivePlayer(ISpecializedPlayerRepository repo, long userId, CancellationToken cancellation)
        =>  async () => await repo.Players
            .AsNoTracking()
            .OnlyActivePlayers()
            .AnyAsync(x => x.UserId == userId, cancellation)
                ? Failure<advisor.Unit>("Game already includes active player from this user.")
                : Success(unit);
}
