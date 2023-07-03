// FIXME

// namespace advisor;

// using System.IO;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using advisor.Persistence;
// using Microsoft.EntityFrameworkCore;

// public interface IGameEnginesRepository {
//     IQueryable<DbGameEngine> Engines { get; }

//     Task<DbGameEngine> GetOneAsync(long engineId, bool withTracking = true, CancellationToken cancellation = default);

//     Task<DbGameEngine> CreateAsync(CancellationToken cancellation = default);
// }

// public class GameEnginesRepository : IGameEnginesRepository {
//     public GameEnginesRepository(ITime time, IUnitOfWork unit) {
//         this.time = time;
//         this.unit = unit;
//         this.db = unit.Database;
//     }

//     private readonly ITime time;
//     private readonly IUnitOfWork unit;
//     private readonly Database db;

//     public IQueryable<DbGameEngine> Engines => db.GameEngines;

//     public Task<DbGameEngine> GetOneAsync(long engineId, bool withTracking = true, CancellationToken cancellation = default) {
//         return db.GameEngines.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Id == engineId, cancellation);
//     }

//     public Task<DbGameEngine> CreateAsync(string name, Stream content, CancellationToken cancellation = default) {
//         var engine = new DbGameEngine {

//         };

//         return db.AddAsync(engine);
//     }
// }
