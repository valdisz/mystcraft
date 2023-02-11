namespace advisor;

using System.Linq;
using System.Threading;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IGameRepository : IReporsitory<DbGame> {
    IQueryable<DbGame> Games { get; }
    DbGame Add(DbGame game);
    void Update(DbGame game);
    AsyncIO<Option<DbGame>> GetOneGame(long gameId, bool withTracking = true, CancellationToken cancellation = default);
}

public class GameRepository : IGameRepository {
    public GameRepository(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public IUnitOfWork UnitOfWork => db;

    public IQueryable<DbGame> Games => db.Games;

    public AsyncIO<Option<DbGame>> GetOneGame(long gameId, bool withTracking = true, CancellationToken cancellation = default)
        => async () => Success(await Games
            .WithTracking(withTracking)
            .SingleOrDefaultAsync(x => x.Id == gameId, cancellation)
            .AsOption()
        );

    public DbGame Add(DbGame game)
        => db.Games.Add(game).Entity;

    public void Update(DbGame game)
        => db.Entry(game).State = EntityState.Modified;
}
