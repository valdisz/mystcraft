namespace advisor.Features;

using System;
using System.Linq;
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
    public GameSyncFactionsHandler(IGameRepository games, IUnitOfWork unit, IHttpClientFactory httpFactory) {
        this.games = games;
        this.unit = unit;
        this.httpFactory = httpFactory;
    }

    private readonly IGameRepository games;
    private readonly IUnitOfWork unit;
    private readonly IHttpClientFactory httpFactory;

    private record PlayerProjection(long Id, int Number);

    public async Task<GameSyncFactionsResult> Handle(GameSyncFactions request, CancellationToken cancellationToken) {
        var game = await games.GetOneAsync(request.GameId, withTracking: false);
        if (game == null) {
            return new GameSyncFactionsResult(false, "Games does not exist.");
        }

        var playersRepo = unit.Players(game);
        var factions = await playersRepo.Players
            .AsNoTracking()
            .Select(x => new PlayerProjection(x.Id, x.Number))
            .ToListAsync(cancellationToken);

        async Task<DbPlayer> getPlayerAsync(int number, CancellationToken cancellation) {
            var i = factions.FindIndex(x => x.Number == number);
            if (i >= 0) {
                var item = factions[i];
                factions.RemoveAt(i);

                return await playersRepo.GetOneAsync(item.Id);
            }

            return null;
        }

        await unit.BeginTransactionAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;

        // new and updated factions
        var remote = new NewOriginsClient(game.Options.ServerAddress, httpFactory);
        await foreach (var faction in remote.ListFactionsAsync(cancellationToken)) {
            if (!faction.Number.HasValue) {
                continue;
            }

            var player = await getPlayerAsync(faction.Number.Value, cancellationToken);
            if (player == null) {
                player = await playersRepo.AddRemoteAsync(faction.Number.Value, faction.Name, cancellationToken);
            }

            if (player.NextTurnNumber != null) {
                DbPlayerTurn turn = await playersRepo.GetPlayerTurnAsync(player.Id, player.NextTurnNumber.Value, cancellationToken);

                if (faction.OrdersSubmitted && !turn.IsOrdersSubmitted) {
                    turn.OrdersSubmittedAt = now;
                }

                if (faction.TimesSubmitted && !turn.IsTimesSubmitted) {
                    turn.TimesSubmittedAt = now;
                }
            }
        }

        // quit factions
        foreach (var faction in factions) {
            await playersRepo.QuitAsync(faction.Id, cancellationToken);
        }

        await unit.SaveChangesAsync();
        await unit.CommitTransactionAsync(cancellationToken);

        return new GameSyncFactionsResult(true, Game: game);
    }
}

