namespace advisor;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IEnginesRepository {
    IQueryable<DbGameEngine> AllEngines { get; }

    Task<DbGameEngine> GetOneAsync(long engineId, CancellationToken cancellation = default);
    Task<DbGameEngine> GetOneNoTrackingAsync(long engineId, CancellationToken cancellation = default);
}

public class EnginesRepository : IEnginesRepository {
    public EnginesRepository(IUnitOfWork unit, Database db) {
        this.unit = unit;
        this.db = db;
    }

    private readonly IUnitOfWork unit;
    private readonly Database db;

    public IQueryable<DbGameEngine> AllEngines => db.GameEngines;

    public Task<DbGameEngine> GetOneAsync(long engineId, CancellationToken cancellation = default) => AllEngines.SingleOrDefaultAsync(x => x.Id == engineId, cancellation);
    public Task<DbGameEngine> GetOneNoTrackingAsync(long engineId, CancellationToken cancellation = default) => AllEngines.AsNoTracking().SingleOrDefaultAsync(x => x.Id == engineId, cancellation);
}
