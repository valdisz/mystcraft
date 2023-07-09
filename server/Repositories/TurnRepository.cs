namespace advisor;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Model;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface ITurnRepository {
    IQueryable<DbTurn> AllTurns { get; }

    Task<DbTurn> GetOneAsync(int number, CancellationToken cancellation = default);
    Task<DbTurn> GetOneNoTrackingAsync(int number, CancellationToken cancellation = default);

    Task<DbTurn> AddTurnAsync(int turnNumber, CancellationToken cancellation = default);

    Task<DbReport> AddReportAsync(long playerId, int factionNumber, int turnNumber, byte[] data, CancellationToken cancellation = default);

    IQueryable<DbReport> GetReports(int turnNumber);
}

public class TurnRepository : ITurnRepository {
    public TurnRepository(DbGame game, UnitOfWork unit, Database db) {
        this.game = game;
        this.unit = unit;
        this.db = db;
    }

    private readonly DbGame game;
    private readonly UnitOfWork unit;
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
            State = TurnState.PENDING
        };

        await db.Turns.AddAsync(turn, cancellation);

        return turn;
    }

    public async Task<DbReport> AddReportAsync(long playerId, int factionNumber, int turnNumber, byte[] data, CancellationToken cancellation) {
        var report = new DbReport {
            PlayerId = playerId,
            TurnNumber = turnNumber,
            GameId = game.Id,
            FactionNumber = factionNumber,
            Source = data,
        };

        await db.Reports.AddAsync(report, cancellation);

        return report;
    }

    public IQueryable<DbReport> GetReports(int turnNumber)
        => db.Reports.InGame(game).Where(x => x.TurnNumber == turnNumber);
}
