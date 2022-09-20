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

    Task<DbTurn> AddTurnAsync(int turnNumber, CancellationToken cancellation = default);

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

    public async Task<DbTurn> AddTurnAsync(int turnNumber, CancellationToken cancellation) {
        var turn = new DbTurn {
            GameId = game.Id,
            Number = turnNumber,
            Status = TurnStatus.PENDING
        };

        await db.Turns.AddAsync(turn, cancellation);

        return turn;
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
