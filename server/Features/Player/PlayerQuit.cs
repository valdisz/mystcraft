namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;

public record PlayerQuit(long GameId, long PlayerId) : IRequest<PlayerQuitResult>;

public record PlayerQuitResult(bool IsSuccess, string Error = null, DbPlayer Player = null) : IMutationResult;

public class PlayerQuitHandler : IRequestHandler<PlayerQuit, PlayerQuitResult> {
    public PlayerQuitHandler(IGameRepository gameRepo, IPlayerRepository playerRepo) {
        this.gameRepo = gameRepo;
        this.playerRepo = playerRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
    }

    private readonly IUnitOfWork unitOfWork;
    private readonly IGameRepository gameRepo;
    private readonly IPlayerRepository playerRepo;

    public Task<PlayerQuitResult> Handle(PlayerQuit request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.RUNNING } => Success(game),
                { Status: GameStatus.PAUSED } => Success(game),
                _ => Failure<DbGame>("Game must be in RUNNING or PAUSED state.")
            }, cancellationToken))
            .Select(playerRepo.Specialize)
            .Bind(repo => repo.GetOnePlayer(request.PlayerId,
                player => player switch {
                    { IsQuit: true } => Failure<DbPlayer>("Player have quitted."),
                    _ => Success(player)
                }, cancellationToken)
                    .Select(player => player.Quit())
                    .Do(player => repo.Update(player))
            )
            .Bind(player => unitOfWork.CommitTransaction(cancellationToken)
                .Return(player)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbPlayer, PlayerQuitResult>(
                player => new PlayerQuitResult(true, Player: player),
                error => new PlayerQuitResult(false, error.Message),
                cancellationToken
            ));
}
