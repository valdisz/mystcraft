namespace advisor;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface ITurnsRepository {
    IQueryable<DbTurn> AllTurns { get; }

    Task<DbTurn> GetOneAsync(int number, CancellationToken cancellation = default);
    Task<DbTurn> GetOneNoTrackingAsync(int number, CancellationToken cancellation = default);

    Task UpdateTurnNumberAsync(int oldNumber, int newNumber, CancellationToken cancellation = default);
    Task<DbTurn> GetOrCreateNextTurnAsync(int turnNumber, CancellationToken cancellation = default);

    Task<DbReport> AddReportAsync(int factionNumber, int turnNumber, byte[] data, CancellationToken cancellation = default);
}

public class TurnsRepository : ITurnsRepository {
    public TurnsRepository(DbGame game, IUnitOfWork unit, Database db) {
        this.game = game;
        this.unit = unit;
        this.db = db;
    }

    private readonly DbGame game;
    private readonly IUnitOfWork unit;
    private readonly Database db;

    public IQueryable<DbTurn> AllTurns => db.Turns.InGame(game);

    public Task<DbTurn> GetOneAsync(int number, CancellationToken cancellation) {
        return AllTurns.SingleOrDefaultAsync(x => x.Number == number, cancellation);
    }

    public Task<DbTurn> GetOneNoTrackingAsync(int number, CancellationToken cancellation) {
        return AllTurns.AsNoTracking().SingleOrDefaultAsync(x => x.Number == number, cancellation);
    }

    public Task UpdateTurnNumberAsync(int oldNumber, int newNumber, CancellationToken cancellation = default)
    {
        throw new System.NotImplementedException();
    }

    public async Task<DbTurn> GetOrCreateNextTurnAsync(int turnNumber, CancellationToken cancellation) {
        await Task.Delay(0);

        return new DbTurn {
            Number = turnNumber,
            Status = TurnStatus.PENDING
        };
    }

    public async Task<DbReport> AddReportAsync(int factionNumber, int turnNumber, byte[] data, CancellationToken cancellation) {
        var report = new DbReport {
            GameId = game.Id,
            Data = data,
            FactionNumber = factionNumber,
            TurnNumber = turnNumber,
            Parsed = false,
            Imported = false
        };

        await db.Reports.AddAsync(report, cancellation);

        return report;
    }
}
