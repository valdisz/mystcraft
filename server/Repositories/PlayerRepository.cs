namespace advisor;

using System.Linq;
using System.Threading;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IPlayerRepository : IReporsitory<DbPlayer> {
    ISpecializedPlayerRepository Specialize(DbGame game);
}

public interface ISpecializedPlayerRepository : IReporsitory<DbPlayer> {
    IQueryable<DbPlayer> Players { get; }
    IQueryable<DbRegistration> Registrations { get; }
    IQueryable<DbPlayerTurn> PlayerTurns { get; }

    DbRegistration Add(DbRegistration registration);
    DbPlayer Add(DbPlayer player);
    DbPlayerTurn Add(DbPlayer player, DbPlayerTurn playerTurn);

    void Update(DbRegistration registration);
    void Update(DbPlayer player);
    void Update(DbPlayerTurn playerTurn);

    AsyncIO<Option<DbPlayer>> GetOnePlayer(long playerId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbRegistration>> GetOneRegistration(long registrationId, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbPlayerTurn>> GetOnePlayerTurn(long playerId, int turnNumber, bool withTracking = true, CancellationToken cancellation = default);

    IO<Unit> Delete(DbRegistration registration);
    IO<Unit> Delete(DbPlayer player);
    IO<Unit> Delete(DbPlayerTurn playerTurn);
}

public class PlayerRepository: IPlayerRepository {
    public PlayerRepository(Database db) {
        this.db = db;
    }
    private readonly Database db;

    public IUnitOfWork UnitOfWork => db;

    public ISpecializedPlayerRepository Specialize(DbGame game) => new SpecializePlayersRepository(db, game);

    sealed class SpecializePlayersRepository : ISpecializedPlayerRepository {
        public SpecializePlayersRepository(Database db, DbGame game) {
            this.db = db;
            this.game = game;
        }

        private readonly DbGame game;
        private readonly Database db;

        public IUnitOfWork UnitOfWork => db;

        public IQueryable<DbPlayer> Players => db.Players.InGame(game);

        public IQueryable<DbRegistration> Registrations => db.Registrations.InGame(game);

        public IQueryable<DbPlayerTurn> PlayerTurns => db.PlayerTurns.InGame(game);

        public DbRegistration Add(DbRegistration registration) {
            registration.Game = game;
            registration.GameId = game.Id;

            game.Registrations.Add(registration);

            return db.Registrations.Add(registration).Entity;
        }

        public DbPlayer Add(DbPlayer player) {
            player.Game = game;
            player.GameId = game.Id;

            game.Players.Add(player);

            return db.Players.Add(player).Entity;
        }

        public DbPlayerTurn Add(DbPlayer player, DbPlayerTurn playerTurn) {
            playerTurn.Player = player;
            playerTurn.PlayerId = player.Id;

            player.Turns.Add(playerTurn);

            return db.PlayerTurns.Add(playerTurn).Entity;
        }

        public void Update(DbRegistration registration)
            => db.Entry(registration).State = EntityState.Modified;

        public void Update(DbPlayer player)
            => db.Entry(player).State = EntityState.Modified;

        public void Update(DbPlayerTurn playerTurn)
            => db.Entry(playerTurn).State = EntityState.Modified;

        public AsyncIO<Option<DbPlayer>> GetOnePlayer(long playerId, bool withTracking = true, CancellationToken cancellation = default)
            => async () => Success(await Players
                .WithTracking(withTracking)
                .SingleOrDefaultAsync(x => x.Id == playerId, cancellation)
                .AsOption()
            );

        public AsyncIO<Option<DbRegistration>> GetOneRegistration(long registrationId, bool withTracking = true, CancellationToken cancellation = default)
            => async () => Success(await Registrations
                .WithTracking(withTracking)
                .SingleOrDefaultAsync(x => x.Id == registrationId, cancellation)
                .AsOption()
            );

        public AsyncIO<Option<DbPlayerTurn>> GetOnePlayerTurn(long playerId, int turnNumber, bool withTracking = true, CancellationToken cancellation = default)
            => async () => Success(await PlayerTurns
                .WithTracking(withTracking)
                .SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber, cancellation)
                .AsOption()
            );

        public IO<Unit> Delete(DbRegistration registration)
            => () => {
                db.Entry(registration).State = EntityState.Deleted;
                return Success(unit);
            };

        public IO<Unit> Delete(DbPlayer player)
            => () => {
                db.Entry(player).State = EntityState.Deleted;
                return Success(unit);
            };

        public IO<Unit> Delete(DbPlayerTurn playerTurn)
            => () => {
                db.Entry(playerTurn).State = EntityState.Deleted;
                return Success(unit);
            };


        // public Task<DbRegistration> GetRegistrationAsync(long registrationId, bool withTracking = true, CancellationToken cancellation = default)
        //     => Registrations.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Id == registrationId, cancellation);

        // public Task<DbPlayer> GetPlayerAsync(long playerId, bool withTracking = true, CancellationToken cancellation = default)
        //     => Players.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Id == playerId, cancellation);

        // public Task<DbPlayer> GetPlayerByUserAsync(long userId, bool withTracking = true, CancellationToken cancellation = default)
        //     => Players.WithTracking(withTracking).SingleOrDefaultAsync(x => x.UserId == userId, cancellation);

        // public Task<DbPlayer> GetPlayerByNumberAsync(int number, bool withTracking = true, CancellationToken cancellation = default)
        //     => Players.WithTracking(withTracking).SingleOrDefaultAsync(x => x.Number == number, cancellation);

        // public Task<DbPlayerTurn> GetPlayerTurnAsync(long playerId, int turnNumber, CancellationToken cancellation = default)
        //     => db.PlayerTurns.SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber, cancellation);

        // public Task<DbPlayerTurn> GetPlayerTurnNoTrackingAsync(long playerId, int turnNumber, CancellationToken cancellation = default)
        //     => db.PlayerTurns.AsNoTracking().SingleOrDefaultAsync(x => x.PlayerId == playerId && x.TurnNumber == turnNumber, cancellation);

        // public async Task<DbRegistration> RegisterPlayerAsync(long userId, string name, string password, CancellationToken cancellation = default) {
        //     var reg = new DbRegistration {
        //         GameId = gameId,
        //         UserId = userId,
        //         Name = name,
        //         Password = password
        //     };

        //     await db.Registrations.AddAsync(reg, cancellation);

        //     return reg;
        // }

        // public async Task<DbPlayer> AddLocalAsync(long registrationId, int factionNumber, CancellationToken cancellation = default) {
        //     if (await GetPlayerByNumberAsync(factionNumber, withTracking: false) is not null) {
        //         throw new RepositoryException("Player already exist");
        //     }

        //     var reg = await GetRegistrationAsync(registrationId, cancellation: cancellation);

        //     var player = new DbPlayer {
        //         GameId = reg.GameId,
        //         UserId = reg.UserId,
        //         Name = reg.Name,
        //         Number = factionNumber,
        //         Password = reg.Password
        //     };

        //     db.Remove(reg);
        //     await db.Players.AddAsync(player, cancellation);

        //     return player;
        // }

        // public async Task<DbPlayer> AddRemoteAsync(int number, string name, CancellationToken cancellation = default) {
        //     if (await GetPlayerByNumberAsync(number, withTracking: false) is not null) {
        //         throw new RepositoryException("Player already exist");
        //     }

        //     var player = new DbPlayer {
        //         GameId = gameId,
        //         Number = number,
        //         Name = name
        //     };

        //     await db.Players.AddAsync(player, cancellation);

        //     return player;
        // }

        // public async Task<DbPlayer> QuitAsync(long playerId, CancellationToken cancellation = default) {
        //     var player = await GetPlayerAsync(playerId);
        //     if (player is null) {
        //         throw new RepositoryException($"Player does not exist");
        //     }

        //     player.IsQuit = true;

        //     return player;
        // }

        // public async Task DeleteRegistrationAsync(long registrationId, CancellationToken cancellation = default) {
        //     var reg = await GetRegistrationAsync(registrationId, cancellation: cancellation);
        //     if (reg is null) {
        //         throw new RepositoryException($"Registration does not exist");
        //     }

        //     db.Remove(reg);
        // }

        // public async Task DeletePlayerAsync(long playerId, CancellationToken cancellation = default) {
        //     var reg = await GetPlayerAsync(playerId, cancellation: cancellation);
        //     if (reg is null) {
        //         throw new RepositoryException($"Player does not exist");
        //     }

        //     db.Remove(reg);
        // }
    }
}
