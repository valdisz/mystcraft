namespace advisor;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using advisor.Model;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IPlayerRepository {
    IQueryable<DbOrders> Orders(int turnNumber);
}

public class PlayerRepository : IPlayerRepository {
    public PlayerRepository(DbPlayer player, IUnitOfWork unit, Database db) {
        this.player = player;
        this.unit = unit;
        this.db = db;
    }

    private readonly DbPlayer player;
    private readonly IUnitOfWork unit;
    private readonly Database db;

    public IQueryable<DbOrders> Orders(int turnNumber) => db.Orders.InTurn(player.Id, turnNumber);
}

public interface IPlayersRepository {
    IQueryable<DbPlayer> AllPlayers { get; }
    IQueryable<DbPlayer> AllActivePlayers { get; }

    Task<DbPlayer> AddLocalAsync(string name, long userId, CancellationToken cancellation = default);
    Task<DbPlayer> AddRemoteAsync(int number, string name, CancellationToken cancellation = default);
    Task<DbPlayer> ClamFactionAsync(string reportText, long userId, long playerId, string password, CancellationToken cancellation = default);
    Task<DbPlayer> QuitAsync(long playerId, CancellationToken cancellation = default);

    Task<DbPlayer> GetOneAsync(long playerId, CancellationToken cancellation = default);
    Task<DbPlayer> GetOneNoTrackingAsync(long playerId, CancellationToken cancellation = default);

    Task<DbPlayer> GetOneByUserAsync(long userId, CancellationToken cancellation = default);
    Task<DbPlayer> GetOneByUserNoTrackingAsync(long userId, CancellationToken cancellation = default);

    Task<DbPlayer> GetOneByNumberAsync(int number, CancellationToken cancellation = default);
    Task<DbPlayer> GetOneByNumberNoTrackingAsync(int number, CancellationToken cancellation = default);

    Task<DbPlayerTurn> GetPlayerTurnAsync(long playerTurnId, CancellationToken cancellation = default);
    Task<DbPlayerTurn> GetPlayerTurnNoTrackingAsync(long playerTurnId, CancellationToken cancellation = default);
}

[System.Serializable]
public class PlayersRepositoryException : System.Exception
{
    public PlayersRepositoryException() { }
    public PlayersRepositoryException(string message) : base(message) { }
    public PlayersRepositoryException(string message, System.Exception inner) : base(message, inner) { }
    protected PlayersRepositoryException(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

public class PlayersRepository : IPlayersRepository {
    public PlayersRepository(DbGame game, IUnitOfWork unit, Database db) {
        this.game = game;
        this.unit = unit;
        this.db = db;
    }

    private readonly DbGame game;
    private readonly IUnitOfWork unit;
    private readonly Database db;

    public async Task<DbPlayer> AddLocalAsync(string name, long userId, CancellationToken cancellation = default) {
        if (game.Type != GameType.LOCAL) {
            throw new PlayersRepositoryException("Local players can be added just to the local game.");
        }

        if (game.Status == GameStatus.COMPLEATED) {
            throw new PlayersRepositoryException("Game already ended.");
        }

        var player = await GetOneByUserNoTrackingAsync(userId, cancellation);
        if (player != null) {
            throw new PlayersRepositoryException("Only one player allowed per user.");
        }

        player = new DbPlayer {
            GameId = game.Id,
            UserId = userId,
            Name = name,

            // there will be a random password
            Password = Guid.NewGuid().ToString("N")
        };

        await db.Players.AddAsync(player, cancellation);

        return player;
    }

    public async Task<DbPlayer> AddRemoteAsync(int number, string name, CancellationToken cancellation = default) {
        if (game.Type != GameType.REMOTE) {
            throw new PlayersRepositoryException("Remote players can be added just to the remote game.");
        }

        var player = await GetOneByNumberNoTrackingAsync(number, cancellation);
        if (player != null) {
            throw new PlayersRepositoryException($"Remote faction with number {number} is already added.");
        }

        await unit.BeginTransactionAsync(cancellation);

        player = new DbPlayer {
            GameId = game.Id,
            Number = number,
            Name = name
        };

        await db.Players.AddAsync(player, cancellation);
        await db.SaveChangesAsync(cancellation);

        var lastTurn = new DbPlayerTurn {
            PlayerId = player.Id,
            TurnNumber = game.LastTurnNumber.Value
        };

        var nextTurn = new DbPlayerTurn {
            PlayerId = player.Id,
            TurnNumber = game.NextTurnNumber.Value
        };

        player.LastTurn = lastTurn;
        player.NextTurn = nextTurn;

        await db.PlayerTurns.AddAsync(lastTurn, cancellation);
        await db.PlayerTurns.AddAsync(nextTurn, cancellation);
        await db.SaveChangesAsync(cancellation);

        await unit.CommitTransactionAsync(cancellation);

        return player;
    }

    public async Task<DbPlayer> ClamFactionAsync(string reportText, long userId, long playerId, string password, CancellationToken cancellation = default) {
        var player = await GetOneAsync(playerId);
        if (player.UserId.HasValue) {
            throw new PlayersRepositoryException($"Faction {player.Number} already claimed.");
        }

        if ((await GetOneByUserNoTrackingAsync(userId)) != null) {
            throw new PlayersRepositoryException($"User already have claimed control over another faction.");
        }

        var (number, name) = await ReadPlayerFromReportAsync(reportText);

        var report = new DbReport {
            GameId = game.Id,
            TurnNumber = game.LastTurnNumber.Value,
            FactionNumber = number,
            Data = Encoding.UTF8.GetBytes(reportText)
        };

        player.UserId = userId;
        player.Password = password;

        await db.Reports.AddAsync(report, cancellation);

        // await db.SaveChangesAsync(cancellation);

        return player;
    }

    public async Task<DbPlayer> QuitAsync(long playerId, CancellationToken cancellation = default) {
        var player = await GetOneAsync(playerId);
        if (player == null) {
            throw new PlayersRepositoryException($"Player does not exist.");
        }

        player.IsQuit = true;

        return player;
    }

    public IQueryable<DbPlayer> AllPlayers => db.Players.InGame(game);
    public IQueryable<DbPlayer> AllActivePlayers => AllPlayers.OnlyActivePlayers();

    public Task<DbPlayer> GetOneAsync(long playerId, CancellationToken cancellation = default) => AllPlayers.SingleOrDefaultAsync(x => x.Id == playerId, cancellation);
    public Task<DbPlayer> GetOneNoTrackingAsync(long playerId, CancellationToken cancellation = default) => AllPlayers.AsNoTracking().SingleOrDefaultAsync(x => x.Id == playerId, cancellation);

    public Task<DbPlayer> GetOneByUserAsync(long userId, CancellationToken cancellation = default) => AllPlayers.SingleOrDefaultAsync(x => x.UserId == userId, cancellation);
    public Task<DbPlayer> GetOneByUserNoTrackingAsync(long userId, CancellationToken cancellation = default) => AllPlayers.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellation);

    public Task<DbPlayer> GetOneByNumberAsync(int number, CancellationToken cancellation = default) => AllPlayers.SingleOrDefaultAsync(x => x.Number == number, cancellation);
    public Task<DbPlayer> GetOneByNumberNoTrackingAsync(int number, CancellationToken cancellation = default) => AllPlayers.AsNoTracking().SingleOrDefaultAsync(x => x.Number == number, cancellation);

    public Task<DbPlayerTurn> GetPlayerTurnAsync(long playerTurnId, CancellationToken cancellation = default) => db.PlayerTurns.SingleOrDefaultAsync(x => x.Id == playerTurnId, cancellation);
    public Task<DbPlayerTurn> GetPlayerTurnNoTrackingAsync(long playerTurnId, CancellationToken cancellation = default) => db.PlayerTurns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == playerTurnId, cancellation);

    private async Task<(int number, string name)> ReadPlayerFromReportAsync(string source) {
        using var textReader = new StringReader(source);

        using var atlantisReader = new AtlantisReportJsonConverter(textReader,
            new ReportFactionSection()
        );
        var json = await atlantisReader.ReadAsJsonAsync();
        var report  = json.ToObject<JReport>();

        var faction = report.Faction;

        return (faction.Number, faction.Name);
    }
}
