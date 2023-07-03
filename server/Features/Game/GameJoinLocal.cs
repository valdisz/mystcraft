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
    public GameJoinLocalHandler(IAllGamesRepository gameRepo, IPlayerRepository playerRepo) {
        this.gameRepo = gameRepo;
        this.playerRepo = playerRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
    }

    private readonly IAllGamesRepository gameRepo;
    private readonly IPlayerRepository playerRepo;
    private readonly IUnitOfWork unitOfWork;

    public Task<GameJoinLocalResult> Handle(GameJoinLocal request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
            .Validate(game => game switch {
                { Type: Persistence.GameType.REMOTE } => Failure<DbGame>("Cannot join remote game."),
                { Status: GameStatus.NEW } => Failure<DbGame>("Game not yet started."),
                { Status: GameStatus.LOCKED } => Failure<DbGame>("Game is processing a turn."),
                { Status: GameStatus.STOPED }  => Failure<DbGame>("Game compleated."),
                _ => Success(game)
            })
            .Select(game => playerRepo.Specialize(game))
            .Bind(repo => DoesNotHaveRegistration(repo, request.UserId, cancellationToken)
                .Bind(_ => DoesNotHaveActivePlayer(repo, request.UserId, cancellationToken))
                .Select(_ => repo.Add(DbRegistration.Create(request.UserId, request.Name, Guid.NewGuid().ToString("N"))))
            )
            .RunWithRollback(
                unitOfWork,
                reg => new GameJoinLocalResult(true, Registration: reg),
                error => new GameJoinLocalResult(false, error.Message),
                cancellationToken
            );

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
