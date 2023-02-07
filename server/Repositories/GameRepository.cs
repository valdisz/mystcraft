namespace advisor;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IGameRepository : IReporsitory<DbGame> {
    DbGame Add(DbGame game);
    DbRegistration Add(DbGame game, DbRegistration registration);
    void Update(DbGame game);
    void Update(DbPlayer player);
    AsyncIO<Option<DbGame>> GetOneGame(long gameId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbPlayer>> GetOnePlayer(long gameId, long playerId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbPlayer>> GetOnePlayer(DbGame game, long playerId, bool withTracking = true, CancellationToken cancellation = default);
    IQueryable<DbGame> QueryGames(long gameId);
    IQueryable<DbRegistration> QueryRegistrations(long gameId);
    IQueryable<DbRegistration> QueryRegistrations(DbGame game);
    IQueryable<DbPlayer> QueryPlayers(long gameId);
    IQueryable<DbPlayer> QueryPlayers(DbGame game);
}

public class GameRepository : IGameRepository {
    public GameRepository(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public IUnitOfWork UnitOfWork => db;

    public IQueryable<DbGame> QueryGames(long gameId) => db.Games.Where(x => x.Id == gameId);

    public AsyncIO<Option<DbGame>> GetOneGame(long gameId, bool withTracking = true, CancellationToken cancellation = default)
        => async () => Success(await QueryGames(gameId)
            .WithTracking(withTracking)
            .SingleOrDefaultAsync(cancellation)
            .AsOptionAsync());

    public AsyncIO<Option<DbPlayer>> GetOnePlayer(long gameId, long playerId, bool withTracking = true, CancellationToken cancellation = default)
        => async () => Success(await QueryPlayers(gameId)
            .WithTracking(withTracking)
            .SingleOrDefaultAsync(x => x.Id == playerId, cancellation)
            .AsOptionAsync());

    public AsyncIO<Option<DbPlayer>> GetOnePlayer(DbGame game, long playerId, bool withTracking = true, CancellationToken cancellation = default)
        => GetOnePlayer(game.Id, playerId, withTracking, cancellation);

    public DbGame Add(DbGame game) {
        return db.Games.Add(game).Entity;
    }

    public void Update(DbGame game) {
        db.Entry(game).State = EntityState.Modified;
    }

    public void Update(DbPlayer player) {
        db.Entry(player).State = EntityState.Modified;
    }

    public DbRegistration Add(DbGame game, DbRegistration registration) {
        registration.Game = game;
        registration.GameId = game.Id;

        game.Registrations.Add(registration);

        return db.Registrations.Add(registration).Entity;
    }

    public IQueryable<DbRegistration> QueryRegistrations(long gameId) => db.Registrations.InGame(gameId);

    public IQueryable<DbRegistration> QueryRegistrations(DbGame game) => QueryRegistrations(game.Id);

    public IQueryable<DbPlayer> QueryPlayers(long gameId) => db.Players.InGame(gameId);

    public IQueryable<DbPlayer> QueryPlayers(DbGame game) => QueryPlayers(game.Id);
}
