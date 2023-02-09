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
    public GameSyncFactionsHandler(IGameRepository gameRepo, IHttpClientFactory httpFactory) {
        this.gameRepo = gameRepo;
        this.unitOfWork = gameRepo.UnitOfWork;
        this.httpFactory = httpFactory;
    }

    private readonly IGameRepository gameRepo;
    private readonly IUnitOfWork unitOfWork;
    private readonly IHttpClientFactory httpFactory;

    public  Task<GameSyncFactionsResult> Handle(GameSyncFactions request, CancellationToken cancellationToken)
        => unitOfWork.BeginTransaction(cancellationToken)
            .Bind(() => gameRepo.GetOneGame(request.GameId, game => game switch {
                { Status: GameStatus.RUNNING } => Success(game),
                { Status: GameStatus.PAUSED } => Success(game),
                _ => Failure<DbGame>("Game must be in RUNNING or PAUSED state.")
            }, cancellationToken))
            .Bind(game => SyncWithRemotePlayers(request.GameId, game.Options.ServerAddress, cancellationToken)
                .Return(game)
            )
            .PipeTo(unitOfWork.RunWithRollback<DbGame, GameSyncFactionsResult>(
                game => new GameSyncFactionsResult(true, Game: game),
                error => new GameSyncFactionsResult(false, error.Message),
                cancellationToken
            ));

    // todo: rewrite this to functional style
    private AsyncIO<advisor.Unit> SyncWithRemotePlayers(long gameId, string serverAddress, CancellationToken cancellation)
        => async () => {
            var players = await gameRepo.QueryPlayers(gameId)
                .OnlyActivePlayers()
                .ToDictionaryAsync(x => x.Number, cancellation);

            DbPlayer popPlayer(int number) {
                if (players.TryGetValue(number, out var p)) {
                    players.Remove(number);
                    return p;
                }

                return null;
            }

            // todo: remote game server client must not be hardcoded
            IRemoteGame remote = new NewOrigins(serverAddress, httpFactory);
            await foreach (var faction in remote.ListFactionsAsync(cancellation)) {
                if (!faction.Number.HasValue) {
                    continue;
                }

                var player = popPlayer(faction.Number.Value);
                if (player == null) {
                    // todo: add new player
                    // player = await gameRepo.AddRemoteAsync(faction.Number.Value, faction.Name, cancellation);
                }

                if (player.NextTurnNumber != null) {
                    DbPlayerTurn turn = await playersRepo.GetPlayerTurnAsync(player.Id, player.NextTurnNumber.Value, cancellation);

                    if (faction.OrdersSubmitted && !turn.IsOrdersSubmitted) {
                        turn.OrdersSubmittedAt = now;
                    }

                    if (faction.TimesSubmitted && !turn.IsTimesSubmitted) {
                        turn.TimesSubmittedAt = now;
                    }
                }
            }

            // everyone left in the players list does not present in the remote game
            return QuitPlayers(players.Values);
        };

    private Result<advisor.Unit> QuitPlayers(IEnumerable<DbPlayer> players) {
        foreach (var player in players) {
            if (player.Quit() is Result<advisor.Unit>.Failure failure) {
                return failure;
            }

            gameRepo.Update(player);
        }

        return Success(unit);
    }

        // var game = await games.GetOneGame(request.GameId, withTracking: false);
        // if (game == null) {
        //     return new GameSyncFactionsResult(false, "Games does not exist.");
        // }

        // var playersRepo = unit.Players(game);
        // var factions = await playersRepo.Players
        //     .AsNoTracking()
        //     .Select(x => new PlayerProjection(x.Id, x.Number))
        //     .ToListAsync(cancellationToken);

        // async Task<DbPlayer> getPlayerAsync(int number, CancellationToken cancellation) {
        //     var i = factions.FindIndex(x => x.Number == number);
        //     if (i >= 0) {
        //         var item = factions[i];
        //         factions.RemoveAt(i);

        //         return await playersRepo.GetOneAsync(item.Id);
        //     }

        //     return null;
        // }

        // await unit.BeginTransaction(cancellationToken);

        // var now = DateTimeOffset.UtcNow;

        // // new and updated factions
        // var remote = new NewOriginsClient(game.Options.ServerAddress, httpFactory);
        // await foreach (var faction in remote.ListFactionsAsync(cancellationToken)) {
        //     if (!faction.Number.HasValue) {
        //         continue;
        //     }

        //     var player = await getPlayerAsync(faction.Number.Value, cancellationToken);
        //     if (player == null) {
        //         player = await playersRepo.AddRemoteAsync(faction.Number.Value, faction.Name, cancellationToken);
        //     }

        //     if (player.NextTurnNumber != null) {
        //         DbPlayerTurn turn = await playersRepo.GetPlayerTurnAsync(player.Id, player.NextTurnNumber.Value, cancellationToken);

        //         if (faction.OrdersSubmitted && !turn.IsOrdersSubmitted) {
        //             turn.OrdersSubmittedAt = now;
        //         }

        //         if (faction.TimesSubmitted && !turn.IsTimesSubmitted) {
        //             turn.TimesSubmittedAt = now;
        //         }
        //     }
        // }

        // // quit factions
        // foreach (var faction in factions) {
        //     await playersRepo.QuitAsync(faction.Id, cancellationToken);
        // }

        // await unit.SaveChanges();
        // await unit.CommitTransaction(cancellationToken);

        // return new GameSyncFactionsResult(true, Game: game);
}

