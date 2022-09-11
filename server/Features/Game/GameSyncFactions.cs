namespace advisor.Features;

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
public record GameSyncFactionsResult(bool IsSuccess, string Error = null, DbGame Game = null) : IMutationResult;

public class GameSyncFactionsJob {
    public GameSyncFactionsJob(IMediator mediator) {
        this.mediator = mediator;
    }

    private readonly IMediator mediator;

    public Task RunAsync(long gameId) => mediator.Send(new GameSyncFactions(gameId));
}

public class GameSyncFactionsHandler : IRequestHandler<GameSyncFactions, GameSyncFactionsResult> {
    public GameSyncFactionsHandler(IUnitOfWork unit, IHttpClientFactory httpFactory) {
        this.unit = unit;
        this.httpFactory = httpFactory;
    }

    private readonly IUnitOfWork unit;
    private readonly IHttpClientFactory httpFactory;

    private record PlayerProjection(long Id, int Number);

    public async Task<GameSyncFactionsResult> Handle(GameSyncFactions request, CancellationToken cancellationToken) {
        var game = await unit.Games.GetOneNoTrackingAsync(request.GameId);
        if (game == null) {
            return new GameSyncFactionsResult(false, "Games does not exist.");
        }

        var remote = new NewOriginsClient(game.Options.ServerAddress, httpFactory);
        var players = await unit.PlayersAsync(game, cancellationToken);

        var factions = await players.AllActivePlayers
            .AsNoTracking()
            .Where(x => x.Number.HasValue)
            .Select(x => new PlayerProjection(x.Id, x.Number.Value))
            .ToListAsync(cancellationToken);

        async Task<DbPlayer> getPlayerAsync(int number, CancellationToken cancellation) {
            var i = factions.FindIndex(x => x.Number == number);
            if (i >= 0) {
                var item = factions[i];
                factions.RemoveAt(i);

                return await players.GetOneNoTrackingAsync(item.Id, cancellation);
            }

            return null;
        }

        await unit.BeginTransactionAsync(cancellationToken);

        // new and updated factions
        foreach (var faction in await remote.ListFactionsAsync(cancellationToken)) {
            if (!faction.Number.HasValue) {
                continue;
            }

            var player = await getPlayerAsync(faction.Number.Value, cancellationToken);
            DbPlayerTurn turn;
            if (player == null) {
                player = await players.AddRemoteAsync(faction.Number.Value, faction.Name, cancellationToken);
                turn = player.NextTurn;
            }
            else {
                turn = await players.GetPlayerTurnAsync(player.NextTurnId.Value, cancellationToken);
            }

            turn.OrdersSubmitted = faction.OrdersSubmitted;
            turn.TimesSubmitted = faction.TimesSubmitted;
        }

        // quit factions
        foreach (var faction in factions) {
            await players.QuitAsync(faction.Id, cancellationToken);
        }

        await unit.SaveChangesAsync();
        await unit.CommitTransactionAsync(cancellationToken);

        return new GameSyncFactionsResult(true, Game: game);
    }
}

