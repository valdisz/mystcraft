namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

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

        public Task<List<DbUniversity>> Universities([Parent] DbUserGame game) {
            return db.Universities
                .Where(x => x.GameId == game.Id && x.UniversityUsers.Any(y => y.UserId == game.UserId))
                .ToListAsync();
        }

        public Task<List<DbReport>> Reports([Parent] DbUserGame game, long? turn = null) {
            var q = db.Reports
                .Where(x => x.UserGameId == game.Id);

            if (turn != null) {
                q = q
                    .Include(x => x.Turn)
                    .Where(x => x.Turn.Number == turn);
            }

            return q.ToListAsync();
        }

        public Task<List<DbTurn>> Turns([Parent] DbUserGame game) {
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
