namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record PlayerQuit(long GameId, long PlayerId) : IRequest<PlayerQuitResult>;

public record PlayerQuitResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

public class PlayerQuitHandler : IRequestHandler<PlayerQuit, PlayerQuitResult> {
    public PlayerQuitHandler(IAllGamesRepository gameRepo, IPlayerRepository playerRepo) {
        this.gameRepo = gameRepo;
        this.playerRepo = playerRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
    }

    private readonly IUnitOfWork unitOfWork;
    private readonly IAllGamesRepository gameRepo;
    private readonly IPlayerRepository playerRepo;

    public Task<PlayerQuitResult> Handle(PlayerQuit request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, cancellation: cancellationToken))
            .Validate(game => game switch {
                { Status: GameStatus.RUNNING } => Success(game),
                { Status: GameStatus.PAUSED } => Success(game),
                _ => Failure<DbGame>("Game must be in RUNNING or PAUSED state.")
            })
            .Select(playerRepo.Specialize)
            .Bind(repo => repo.GetOnePlayer(request.PlayerId, cancellation: cancellationToken)
                .Validate(player => player switch {
                    { IsQuit: true } => Failure<DbPlayer>("Player quitted."),
                    _ => Success(player)
                })
                .Bind(player => player.Quit())
                .Bind(repo.Update)
            )
            .RunWithRollback(
                unitOfWork,
                player => new PlayerQuitResult(true, Player: player),
                error => new PlayerQuitResult(false, error.Message),
                cancellationToken
            );
}
