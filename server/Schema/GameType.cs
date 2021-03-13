namespace atlantis {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class GameType : ObjectType<DbGame> {
        protected override void Configure(IObjectTypeDescriptor<DbGame> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) =>
                {
                    var db = ctx.Service<Database>();
                    return db.Games.FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Game")]
    public class GameResolvers {
        public GameResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbReport>> GetReports([Parent] DbUserGame game, long? turn = null) {
            var q = db.Reports
                .Where(x => x.UserGameId == game.Id);

            if (turn != null) {
                q = q
                    .Include(x => x.Turn)
                    .Where(x => x.Turn.Number == turn);
            }

            return q.ToListAsync();
        }

        public Task<List<DbTurn>> GetTurns([Parent] DbUserGame game) {
            return db.Turns
                .Where(x => x.UserGameId == game.Id)
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public Task<DbTurn> TurnByNumber([Parent] DbUserGame game, long turn) {
            return db.Turns
                .FirstOrDefaultAsync(x => x.UserGameId == game.Id && x.Number == turn);
        }
    }

    public class UserGameType : ObjectType<DbUserGame> {
        protected override void Configure(IObjectTypeDescriptor<DbUserGame> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) =>
                {
                    var db = ctx.Service<Database>();
                    return db.UserGames.FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "UserGame")]
    public class UserGameResolvers {
        public UserGameResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbReport>> GetReports([Parent] DbUserGame game, long? turn = null) {
            var q = db.Reports
                .Where(x => x.UserGameId == game.Id);

            if (turn != null) {
                q = q
                    .Include(x => x.Turn)
                    .Where(x => x.Turn.Number == turn);
            }

            return q.ToListAsync();
        }

        public Task<List<DbTurn>> GetTurns([Parent] DbUserGame game) {
            return db.Turns
                .Where(x => x.UserGameId == game.Id)
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public Task<DbTurn> TurnByNumber([Parent] DbUserGame game, long turn) {
            return db.Turns
                .FirstOrDefaultAsync(x => x.UserGameId == game.Id && x.Number == turn);
        }
    }
}
