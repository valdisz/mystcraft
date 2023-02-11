namespace advisor.Features;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using advisor.Schema;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record GameSyncFactions(long GameId) : IRequest<GameSyncFactionsResult>;

public record GameSyncFactionsResult(bool IsSuccess, string Error = null, DbGame Game = null) : MutationResult(IsSuccess, Error);

public class GameSyncFactionsHandler : IRequestHandler<GameSyncFactions, GameSyncFactionsResult> {
    public GameSyncFactionsHandler(IGameRepository gameRepo, IPlayerRepository playerRepo, ITime time, IHttpClientFactory httpFactory) {
        this.gameRepo = gameRepo;
        this.playerRepo = playerRepo;
        this.time = time;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.httpFactory = httpFactory;
    }

    private readonly IGameRepository gameRepo;
    private readonly IPlayerRepository playerRepo;
    private readonly ITime time;
    private readonly IUnitOfWork unitOfWork;
    private readonly IHttpClientFactory httpFactory;

    private record struct PlayerAndTurn(DbPlayer player, DbPlayerTurn turn);

    public  Task<GameSyncFactionsResult> Handle(GameSyncFactions request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.RUNNING } => Success(game),
                { Status: GameStatus.PAUSED } => Success(game),
                _ => Failure<DbGame>("Game must be in RUNNING or PAUSED state.")
            }, cancellationToken))
            .Bind(game => SyncWithRemotePlayers(game, game.Options.ServerAddress, cancellationToken)
                .Return(game)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbGame, GameSyncFactionsResult>(
                game => new GameSyncFactionsResult(true, Game: game),
                error => new GameSyncFactionsResult(false, error.Message),
                cancellationToken
            ));

    // todo: rewrite this to fully functional style
    private AsyncIO<advisor.Unit> SyncWithRemotePlayers(DbGame game, string serverAddress, CancellationToken cancellation)
        => async () => {
            var repo = playerRepo.Specialize(game);

            var players = await repo.Players
                .OnlyActivePlayers()
                .ToDictionaryAsync(x => x.Number, cancellation);

            Option<DbPlayer> popPlayer(int number)
                => (players.TryGetValue(number, out var p) ? Some(p) : None<DbPlayer>())
                    .Do(player => players.Remove(player.Number));

            var now = time.UtcNow;
            // todo: remote game server client must not be hardcoded
            IRemoteGame remote = new NewOrigins(serverAddress, httpFactory);
            await foreach (var faction in remote.ListFactionsAsync(cancellation)) {
                if (!faction.Number.HasValue) {
                    continue;
                }

                var result = await popPlayer(faction.Number.Value)
                    .Select(player => repo.GetOnePlayerTurn(player.Id, player.NextTurnNumber.Value, cancellation: cancellation)
                        .Select(maybeTurn => maybeTurn
                            .Select(turn => Success(new PlayerAndTurn(player, turn)))
                            .Unwrap(() => Failure<PlayerAndTurn>("Next player turn does not exist."))
                        )
                    )
                    .Unwrap(() => Effect(() => Success(DbPlayer.CreateRemote(faction.Number.Value, faction.Name))
                        .Do(player => player.NextTurnNumber = game.NextTurnNumber)
                        .Select(player => repo.Add(player))
                        .Bind(player => Success(DbPlayerTurn.Create(faction.Number.Value, faction.Name, game.NextTurnNumber.Value))
                            .Select(turn => repo.Add(player, turn))
                            .Select(turn => new PlayerAndTurn(player, turn))
                        )
                    ))
                    .Do(pt => {
                        var turn = pt.turn;
                        if (faction.OrdersSubmitted && !turn.IsOrdersSubmitted) {
                            turn.OrdersSubmittedAt = now;
                            repo.Update(turn);
                        }

                        if (faction.TimesSubmitted && !turn.IsTimesSubmitted) {
                            turn.TimesSubmittedAt = now;
                            repo.Update(turn);
                        }
                    })
                    .Run();

                if (result is Result<PlayerAndTurn>.Failure(var error)) {
                    return Failure<advisor.Unit>(error);
                }
            }

            // everyone left in the players list does not present in the remote game anymore
            return QuitPlayers(repo, players.Values);
        };

    private Result<advisor.Unit> QuitPlayers(ISpecializedPlayerRepository repo, IEnumerable<DbPlayer> players) {
        foreach (var player in players) {
            if (player.Quit() is Result<advisor.Unit>.Failure failure) {
                return failure;
            }

            repo.Update(player);
        }

        return Success(unit);
    }
}
