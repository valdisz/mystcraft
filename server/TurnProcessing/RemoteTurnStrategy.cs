namespace advisor.TurnProcessing;

using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Remote;
using Microsoft.EntityFrameworkCore;

public class RemoteTurnStrategy {
    public RemoteTurnStrategy(DbGame game, IUnitOfWork unit, IHttpClientFactory httpClientFactory) {
        this.game = game;
        this.unit = unit;
        this.httpClientFactory = httpClientFactory;
    }

    private readonly DbGame game;
    private readonly IUnitOfWork unit;
    private readonly IHttpClientFactory httpClientFactory;

    public async Task RunNextAsync(long gameId, CancellationToken cancellationToken = default) {
        var client = new NewOriginsClient(game.Options.ServerAddress, httpClientFactory);

        

        var remoteTurnNumber = await client.GetCurrentTurnNumberAsync();
        if (remoteTurnNumber == game.LastTurnNumber) {
            // nothing happened on the server
            // todo: fail the strategy
        }

        DbTurn turn;
        if (remoteTurnNumber > game.NextTurnNumber) {
            // we are missing turns, create them
            turn = new DbTurn();
        }
        else {
            turn = await db.Turns.InGame(gameId).SingleAsync(x => x.Number == remoteTurnNumber);
        }

        var factions = (await client.ListFactionsAsync())
            .Where(x => x.Number.HasValue)
            .ToDictionary(x => x.Number.Value);

        var players = await db.Players
            .InGame(gameId)
            .OnlyActivePlayers()
            .ToListAsync(cancellationToken);

        var quitPlayers = players
            .Select(x => x.Number.Value)
            .Except(factions.Keys)
            .ToHashSet();

        // add new players
        foreach (var number in factions.Keys.Except(players.Select(x => x.Number.Value))) {
            var player = new DbPlayer {
                GameId = gameId,
                Number = number
            };

            await db.Players.AddAsync(player, cancellationToken);
            players.Add(player);
        }

        // loop through all active players and add new turn to them
        foreach (var player in players.Where(x => x.Number.HasValue)) {
            var number = player.Number.Value;

            if (quitPlayers.Contains(number)) {
                player.IsQuit = true;
                continue;
            }

            var faction = factions[number];
            player.Name = faction.Name;
            player.LastTurnNumber = turn.Number;
            player.NextTurnNumber = turn.Number + 1;

            var playerTurn = new DbPlayerTurn {
                GameId = gameId,
                TurnNumber = turn.Number,
                PlayerId = player.Id,
            };

            if (!string.IsNullOrWhiteSpace(player.Password)) {
                var text = await client.DownloadReportAsync(player.Number.Value, player.Password);
                var report = new DbReport {
                    GameId = gameId,
                    FactionNumber = player.Number.Value,
                    TurnNumber = turn.Number,
                    Data = Encoding.UTF8.GetBytes(text)
                };

                await db.Reports.AddAsync(report, cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
