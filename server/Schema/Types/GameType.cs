namespace advisor.Schema {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Features;
    using HotChocolate;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate.Data;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Persistence;
    using Remote;

    public class GameType : ObjectType<DbGame> {
        public const string NAME = "Game";

        protected override void Configure(IObjectTypeDescriptor<DbGame> descriptor) {
            descriptor.Name(NAME);

            descriptor
                .ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Games
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(GameType.NAME)]
    public class GameResolvers {
        [UseFirstOrDefault]
        [UseProjection]
        public IQueryable<DbPlayer> Me(Database db, [Parent] DbGame game, [GlobalState] long currentUserId) {
            return db.Players
                .AsNoTracking()
                .InGame(game)
                .Where(x => x.GameId == game.Id && x.UserId == currentUserId);
        }

        public IOrderedQueryable<DbTurn> Turns(Database db, [Parent] DbGame game) {
            return db.Turns
                .InGame(game)
                .OrderBy(x => x.Number);
        }

        // public Task<DbAlliance> Alliance(Database db, [Parent] DbGame game, [GlobalState] long currentUserId) {
        //     return db.Alliances
        //         .Include(x => x.Members)
        //         .Where(x => x.GameId == game.Id))
        //         .SingleOrDefaultAsync();
        // }

        [UseOffsetPaging]
        public IOrderedQueryable<DbPlayer> Players(Database db, [Parent] DbGame game, bool quit = false) {
            var q = db.Players
                .AsNoTracking()
                .InGame(game.Id);

            if (!quit) {
                q = q.Where(x => !x.IsQuit);
            }

            return q.OrderBy(x => x.Id);
        }

        // [Authorize(Policy = Policies.GameMasters)]
        // public Task<List<DbAlliance>> Universities([Parent] DbGame game) {
        //     return db.Universities
        //         .Include(x => x.Members)
        //         .ThenInclude(x => x.Player)
        //         .Where(x => x.GameId == game.Id).ToListAsync();
        // }
    }
}
