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

        public Task<List<DbPlayer>> UserGames([Parent] DbGame game) {
            return db.Players.Where(x => x.GameId == game.Id).ToListAsync();
        }

        public Task<List<DbUniversity>> Universities([Parent] DbGame game) {
            return db.Universities
                .Include(x => x.Members)
                .ThenInclude(x => x.Player)
                .Where(x => x.GameId == game.Id).ToListAsync();
        }
    }
}
