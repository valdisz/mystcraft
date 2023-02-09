namespace advisor;

using System.Linq;
using System.Threading;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IGameRepository : IReporsitory<DbGame> {
    DbGame Add(DbGame game);
    DbRegistration Add(DbGame game, DbRegistration registration);
    DbPlayer Add(DbGame game, DbPlayer player);
    void Update(DbGame game);
    void Update(DbRegistration registration);
    void Update(DbPlayer player);
    AsyncIO<Option<DbGame>> GetOneGame(long gameId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbPlayer>> GetOnePlayer(long gameId, long playerId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbPlayer>> GetOnePlayer(DbGame game, long playerId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbRegistration>> GetOneRegistration(long gameId, long registrationId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbRegistration>> GetOneRegistration(DbGame game, long registrationId, bool withTracking = true, CancellationToken cancellation = default);
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

    public IQueryable<DbRegistration> QueryRegistrations(long gameId)
        => db.Registrations.InGame(gameId);

    public IQueryable<DbPlayer> QueryPlayers(long gameId)
        => db.Players.InGame(gameId);

    public IQueryable<DbRegistration> QueryRegistrations(DbGame game)
        => QueryRegistrations(game.Id);

    public IQueryable<DbPlayer> QueryPlayers(DbGame game)
        => QueryPlayers(game.Id);

    public AsyncIO<Option<DbGame>> GetOneGame(long gameId, bool withTracking = true, CancellationToken cancellation = default)
        => async () => Success(await QueryGames(gameId)
            .WithTracking(withTracking)
            .SingleOrDefaultAsync(cancellation)
            .AsOption()
        );

    public AsyncIO<Option<DbPlayer>> GetOnePlayer(long gameId, long playerId, bool withTracking = true, CancellationToken cancellation = default)
        => async () => Success(await QueryPlayers(gameId)
            .WithTracking(withTracking)
            .SingleOrDefaultAsync(x => x.Id == playerId, cancellation)
            .AsOption()
        );

    public AsyncIO<Option<DbPlayer>> GetOnePlayer(DbGame game, long playerId, bool withTracking = true, CancellationToken cancellation = default)
        => GetOnePlayer(game.Id, playerId, withTracking, cancellation);

    public AsyncIO<Option<DbRegistration>> GetOneRegistration(long gameId, long registrationId, bool withTracking = true, CancellationToken cancellation = default)
        => async () => Success(await QueryRegistrations(gameId)
            .WithTracking(withTracking)
            .SingleOrDefaultAsync(x => x.Id == registrationId, cancellation)
            .AsOption()
        );

    public AsyncIO<Option<DbRegistration>> GetOneRegistration(DbGame game, long registrationId, bool withTracking = true, CancellationToken cancellation = default)
        => GetOneRegistration(game.Id, registrationId, withTracking, cancellation);

    public DbGame Add(DbGame game)
        => db.Games.Add(game).Entity;

    public DbRegistration Add(DbGame game, DbRegistration registration) {
        registration.Game = game;
        registration.GameId = game.Id;

        game.Registrations.Add(registration);

        return db.Registrations.Add(registration).Entity;
    }

    public DbPlayer Add(DbGame game, DbPlayer player) {
        player.Game = game;
        player.GameId = game.Id;

        game.Players.Add(player);

        return db.Players.Add(player).Entity;
    }

    public void Update(DbGame game)
        => db.Entry(game).State = EntityState.Modified;

    public void Update(DbPlayer player)
        => db.Entry(player).State = EntityState.Modified;

    public void Update(DbRegistration registration)
        => db.Entry(registration).State = EntityState.Modified;
}
