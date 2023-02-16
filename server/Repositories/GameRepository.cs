namespace advisor;

using System;
using System.Linq;
using System.Threading;
using advisor.Persistence;
using Microsoft.EntityFrameworkCore;

public interface IGameRepository : IReporsitory<DbGame> {
    IQueryable<DbGame> Games { get; }
    ISpecializedGameRepository Specialize(DbGame game);
    ISpecializedGameRepository Specialize(long gameId);
    IO<DbGame> Add(DbGame game);
    IO<DbGame> Update(DbGame game);
    AsyncIO<Option<DbGame>> GetOneGame(long gameId, bool withTracking = true, CancellationToken cancellation = default);
}

public interface ISpecializedGameRepository {
    IQueryable<DbTurn> Turns { get; }
    IQueryable<DbReport> Reports { get; }
    IQueryable<DbArticle> Articles { get; }

    IO<DbTurn> Add(DbTurn turn);
    IO<DbReport> Add(DbReport report);
    IO<DbArticle> Add(DbArticle article);
    IO<DbTurn> Update(DbTurn turn);
    IO<DbReport> Update(DbReport report);
    IO<DbArticle> Update(DbArticle article);
    AsyncIO<Option<DbTurn>> GetOneTurn(int turnNumber, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbReport>> GetOneReport(int turnNumber, int factionNumber, bool withTracking = true, CancellationToken cancellation = default);
    AsyncIO<Option<DbArticle>> GetOneArticle(long articleId, bool withTracking = true, CancellationToken cancellation = default);
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

    public IO<DbGame> Add(DbGame game)
        => () => Success(db.Games.Add(game).Entity);

    public IO<DbGame> Update(DbGame game)
        => () => {
            db.Entry(game).State = EntityState.Modified;
            return Success(game);
        };

    public ISpecializedGameRepository Specialize(DbGame game)
        => new SpecializedGameRepository(game.Id, db);

    public ISpecializedGameRepository Specialize(long gameId)
        => new SpecializedGameRepository(gameId, db);

    class SpecializedGameRepository : ISpecializedGameRepository {
        public SpecializedGameRepository(long gameId, Database db) {
            this.gameId = gameId;
            this.db = db;
        }

        private readonly Database db;

        private readonly long gameId;

        public IQueryable<DbTurn> Turns => db.Turns.InGame(gameId);

        public IQueryable<DbReport> Reports => db.Reports.InGame(gameId);

        public IQueryable<DbArticle> Articles => db.Articles.InGame(gameId);

        public IO<DbTurn> Add(DbTurn turn)
            => () => Success(db.Turns.Add(turn).Entity);

        public IO<DbReport> Add(DbReport report)
            => () => Success(db.Reports.Add(report).Entity);

        public IO<DbArticle> Add(DbArticle article)
            => () => Success(db.Articles.Add(article).Entity);

        public IO<DbTurn> Update(DbTurn turn)
            => () => {
                db.Entry(turn).State = EntityState.Modified;
                return Success(turn);
            };


        public IO<DbReport> Update(DbReport report)
            => () => {
                db.Entry(report).State = EntityState.Modified;
                return Success(report);
            };


        public IO<DbArticle> Update(DbArticle article)
            => () => {
                db.Entry(article).State = EntityState.Modified;
                return Success(article);
            };

        public AsyncIO<Option<DbTurn>> GetOneTurn(int turnNumber, bool withTracking = true, CancellationToken cancellation = default)
            => AsyncEffect(() => Turns
                .WithTracking(withTracking)
                .SingleOrDefaultAsync(x => x.Number == turnNumber, cancellation)
                .AsOption()
            );

        public AsyncIO<Option<DbReport>> GetOneReport(int turnNumber, int factionNumber, bool withTracking = true, CancellationToken cancellation = default)
            => AsyncEffect(() => Reports
                .WithTracking(withTracking)
                .SingleOrDefaultAsync(x => x.TurnNumber == turnNumber && x.FactionNumber == factionNumber, cancellation)
                .AsOption()
            );

        public AsyncIO<Option<DbArticle>> GetOneArticle(long articleId, bool withTracking = true, CancellationToken cancellation = default)
            => AsyncEffect(() => Articles
                .WithTracking(withTracking)
                .SingleOrDefaultAsync(x => x.Id == articleId, cancellation)
                .AsOption()
            );
    }
}
