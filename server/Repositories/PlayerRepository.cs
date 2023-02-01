namespace advisor;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IPlayerRepository {
    IBoundPlayerRepository For(long gameId);
    IBoundPlayerRepository For(DbGame game);
}

public interface IBoundPlayerRepository {
    IQueryable<DbRegistration> Registrations { get; }
    IQueryable<DbPlayer> Players { get; }

    Task<DbPlayer> GetPlayerAsync(long playerId, bool withTracking = true, CancellationToken cancellation = default);
    Task<DbPlayer> GetPlayerByUserAsync(long userId, bool withTracking = true, CancellationToken cancellation = default);
    Task<DbPlayer> GetPlayerByNumberAsync(int number, bool withTracking = true, CancellationToken cancellation = default);

    Task<DbPlayer> AddLocalAsync(long registrationId, int factionNumber, CancellationToken cancellation = default);
    Task<DbPlayer> AddRemoteAsync(int number, string name, CancellationToken cancellation = default);
    Task<DbPlayer> QuitAsync(long playerId, CancellationToken cancellation = default);
    Task DeleteRegistrationAsync(long registrationId, CancellationToken cancellation = default);
    Task DeletePlayerAsync(long playerId, CancellationToken cancellation = default);
}

public class PlayerRepository: IPlayerRepository {
    public PlayerRepository(IUnitOfWork unit) {
        this.unit = unit;
    }

    private readonly IUnitOfWork unit;

    public IBoundPlayerRepository For(long gameId) => new BoundPlayersRepository(gameId, unit);

    public IBoundPlayerRepository For(DbGame game) => For(game.Id);

    class BoundPlayersRepository : IBoundPlayerRepository {
        public BoundPlayersRepository(long gameId, IUnitOfWork unit) {
            this.gameId = gameId;
            this.unit = unit;
            this.db = unit.Database;
        }

        private readonly long gameId;
        private readonly IUnitOfWork unit;
        private readonly Database db;

        public IQueryable<DbRegistration> Registrations => db.Registrations.InGame(gameId);
        public IQueryable<DbPlayer> Players => db.Players.InGame(gameId);

        public Task<DbRegistration> GetRegistrationAsync(long registrationId, bool withTracking = true, CancellationToken cancellation = default)
            => Registrations.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Id == registrationId, cancellation);

        public Task<DbPlayer> GetPlayerAsync(long playerId, bool withTracking = true, CancellationToken cancellation = default)
            => Players.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Id == playerId, cancellation);

        public Task<DbPlayer> GetPlayerByUserAsync(long userId, bool withTracking = true, CancellationToken cancellation = default)
            => Players.WithTracking(withTracking).SingleOrDefaultAsync(x => x.UserId == userId, cancellation);

        public Task<DbPlayer> GetPlayerByNumberAsync(int number, bool withTracking = true, CancellationToken cancellation = default)
            => Players.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Number == number, cancellation);

        public Task<DbPlayerTurn> GetPlayerTurnAsync(long playerId, int turnNumber, CancellationToken cancellation = default)
            => db.PlayerTurns.SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber, cancellation);

        public Task<DbPlayerTurn> GetPlayerTurnNoTrackingAsync(long playerId, int turnNumber, CancellationToken cancellation = default)
            => db.PlayerTurns.AsNoTracking().SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber, cancellation);

        public async Task<DbRegistration> RegisterPlayerAsync(long userId, string name, string password, CancellationToken cancellation = default) {
            var reg = new DbRegistration {
                GameId = gameId,
                UserId = userId,
                Name = name,
                Password = password
            };

            await db.Registrations.AddAsync(reg, cancellation);

            return reg;
        }

        public async Task<DbPlayer> AddLocalAsync(long registrationId, int factionNumber, CancellationToken cancellation = default) {
            if (await GetPlayerByNumberAsync(factionNumber, withTracking: false) is not null) {
                throw new RepositoryException("Player already exist");
            }

            var reg = await GetRegistrationAsync(registrationId, cancellation: cancellation);

            var player = new DbPlayer {
                GameId = reg.GameId,
                UserId = reg.UserId,
                Name = reg.Name,
                Number = factionNumber,
                Password = reg.Password
            };

            db.Remove(reg);
            await db.Players.AddAsync(player, cancellation);

            return player;
        }

        public async Task<DbPlayer> AddRemoteAsync(int number, string name, CancellationToken cancellation = default) {
            if (await GetPlayerByNumberAsync(number, withTracking: false) is not null) {
                throw new RepositoryException("Player already exist");
            }

            var player = new DbPlayer {
                GameId = gameId,
                Number = number,
                Name = name
            };

            await db.Players.AddAsync(player, cancellation);

            return player;
        }

        public async Task<DbPlayer> QuitAsync(long playerId, CancellationToken cancellation = default) {
            var player = await GetPlayerAsync(playerId);
            if (player is null) {
                throw new RepositoryException($"Player does not exist");
            }

            player.IsQuit = true;

            return player;
        }

        public async Task DeleteRegistrationAsync(long registrationId, CancellationToken cancellation = default) {
            var reg = await GetRegistrationAsync(registrationId, cancellation: cancellation);
            if (reg is null) {
                throw new RepositoryException($"Registration does not exist");
            }

            db.Remove(reg);
        }

        public async Task DeletePlayerAsync(long playerId, CancellationToken cancellation = default) {
            var reg = await GetPlayerAsync(playerId, cancellation: cancellation);
            if (reg is null) {
                throw new RepositoryException($"Player does not exist");
            }

            db.Remove(reg);
        }
    }
}
