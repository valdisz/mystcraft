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
    IQueryable<DbPlayer> Players { get; }
    IQueryable<DbPlayer> ActivePlayers { get; }

    Task<DbPlayer> AddLocalAsync(long registrationId, int factionNumber, CancellationToken cancellation = default);
    Task<DbPlayer> AddRemoteAsync(int number, string name, CancellationToken cancellation = default);
    Task<DbPlayer> ClamFactionAsync(long userId, long playerId, string password, CancellationToken cancellation = default);
    Task<DbPlayer> QuitAsync(long playerId, CancellationToken cancellation = default);

    Task<DbPlayer> GetOneAsync(long playerId);
    Task<DbPlayer> GetOneNoTrackingAsync(long playerId, CancellationToken cancellation = default);

    Task<DbPlayer> GetOneByUserAsync(long userId, CancellationToken cancellation = default);
    Task<DbPlayer> GetOneByUserNoTrackingAsync(long userId, CancellationToken cancellation = default);

    Task<DbPlayer> GetOneByNumberAsync(int number, CancellationToken cancellation = default);
    Task<DbPlayer> GetOneByNumberNoTrackingAsync(int number, CancellationToken cancellation = default);

    Task<DbPlayerTurn> GetPlayerTurnAsync(long playerId, int turnNumber, CancellationToken cancellation = default);
    Task<DbPlayerTurn> GetPlayerTurnNoTrackingAsync(long playerId, int turnNumber, CancellationToken cancellation = default);
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

    public IQueryable<DbPlayer> Players => db.Players.InGame(game);
    public IQueryable<DbPlayer> ActivePlayers => Players.OnlyActivePlayers();

    public async Task<DbPlayer> AddLocalAsync(long registrationId, int factionNumber, CancellationToken cancellation = default) {
        var reg = await unit.Games.GetRegistrationAsync(registrationId);

        if (game.Type != GameType.LOCAL) {
            throw new RepositoryException("Local players can be added just to the local game.");
        }

        if (game.Status == GameStatus.COMPLEATED) {
            throw new RepositoryException("Game already ended.");
        }

        await unit.BeginTransactionAsync(cancellation);

        var player = new DbPlayer {
            GameId = reg.GameId,
            UserId = reg.UserId,
            Name = reg.Name,
            Number = factionNumber,
            Password = reg.Password
        };

        await db.Players.AddAsync(player, cancellation);
        await db.SaveChangesAsync(cancellation);

        var lastTurn = new DbPlayerTurn {
            PlayerId = player.Id,
            TurnNumber = game.LastTurnNumber.Value,
            Name = player.Name
        };

        var nextTurn = new DbPlayerTurn {
            PlayerId = player.Id,
            TurnNumber = game.NextTurnNumber.Value,
            Name = player.Name
        };

        await db.PlayerTurns.AddAsync(lastTurn, cancellation);
        await db.PlayerTurns.AddAsync(nextTurn, cancellation);

        await db.SaveChangesAsync(cancellation);
        await unit.CommitTransactionAsync(cancellation);

        return player;
    }

    public async Task<DbPlayer> AddRemoteAsync(int number, string name, CancellationToken cancellation = default) {
        if (game.Type != GameType.REMOTE) {
            throw new RepositoryException("Remote players can be added just to the remote game.");
        }

        var player = await GetOneByNumberNoTrackingAsync(number, cancellation);
        if (player != null) {
            throw new RepositoryException($"Remote faction with number {number} is already added.");
        }

        await unit.BeginTransactionAsync(cancellation);

        player = new DbPlayer {
            GameId = game.Id,
            Number = number,
            Name = name,
            LastTurnNumber = game.LastTurnNumber,
            NextTurnNumber = game.NextTurnNumber
        };

        await db.Players.AddAsync(player, cancellation);
        await db.SaveChangesAsync(cancellation);

        if (game.LastTurnNumber != null) {
            var lastTurn = new DbPlayerTurn {
                PlayerId = player.Id,
                TurnNumber = game.LastTurnNumber.Value,
                Name = name
            };
            await db.PlayerTurns.AddAsync(lastTurn, cancellation);
        }

        if (game.NextTurnNumber != null) {
            var nextTurn = new DbPlayerTurn {
                PlayerId = player.Id,
                TurnNumber = game.NextTurnNumber.Value,
                Name = name
            };
            await db.PlayerTurns.AddAsync(nextTurn, cancellation);
        }

        await db.SaveChangesAsync(cancellation);
        await unit.CommitTransactionAsync(cancellation);

        return player;
    }

    public async Task<DbPlayer> ClamFactionAsync(long userId, long playerId, string password, CancellationToken cancellation = default) {
        if ((await GetOneByUserNoTrackingAsync(userId)) != null) {
            throw new RepositoryException($"User already have claimed control over another faction.");
        }

        var player = await GetOneAsync(playerId);
        if (player.IsClaimed) {
            throw new RepositoryException($"Faction {player.Number} already claimed.");
        }

        player.UserId = userId;
        player.Password = password;

        return player;
    }

    public async Task<DbPlayer> QuitAsync(long playerId, CancellationToken cancellation = default) {
        var player = await GetOneAsync(playerId);
        if (player == null) {
            throw new RepositoryException($"Player does not exist.");
        }

        player.IsQuit = true;

        return player;
    }

    public async Task<DbPlayer> GetOneAsync(long playerId) => await db.Players.FindAsync(playerId);

    public Task<DbPlayer> GetOneNoTrackingAsync(long playerId, CancellationToken cancellation = default)
        => Players.AsNoTracking().SingleOrDefaultAsync(x => x.Id == playerId, cancellation);

    public Task<DbPlayer> GetOneByUserAsync(long userId, CancellationToken cancellation = default)
        => Players.SingleOrDefaultAsync(x => x.UserId == userId, cancellation);

    public Task<DbPlayer> GetOneByUserNoTrackingAsync(long userId, CancellationToken cancellation = default)
        => Players.AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, cancellation);

    public Task<DbPlayer> GetOneByNumberAsync(int number, CancellationToken cancellation = default)
        => Players.SingleOrDefaultAsync(x => x.Number == number, cancellation);

    public Task<DbPlayer> GetOneByNumberNoTrackingAsync(int number, CancellationToken cancellation = default)
        => Players.AsNoTracking().SingleOrDefaultAsync(x => x.Number == number, cancellation);

    public Task<DbPlayerTurn> GetPlayerTurnAsync(long playerId, int turnNumber, CancellationToken cancellation = default)
        => db.PlayerTurns.SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber, cancellation);

    public Task<DbPlayerTurn> GetPlayerTurnNoTrackingAsync(long playerId, int turnNumber, CancellationToken cancellation = default)
        => db.PlayerTurns.AsNoTracking().SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber, cancellation);
}
