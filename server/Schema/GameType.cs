namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Features;
    using HotChocolate;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Persistence;

    public class GameType : ObjectType<DbGame> {
        protected override void Configure(IObjectTypeDescriptor<DbGame> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode((ctx, id) =>
                {
                    var db = ctx.Service<Database>();
                    return db.Games
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType("Game")]
    public class GameResolvers {
        public Task<DbPlayer> Me(Database db, [Parent] DbGame game, [GlobalState] long currentUserId) {
            return db.Players
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.GameId == game.Id && x.UserId == currentUserId);
        }

        // public Task<DbAlliance> Alliance(Database db, [Parent] DbGame game, [GlobalState] long currentUserId) {
        //     return db.Alliances
        //         .Include(x => x.Members)
        //         .Where(x => x.GameId == game.Id))
        //         .SingleOrDefaultAsync();
        // }

        [Authorize(Policy = Policies.GameMasters)]
        [UseOffsetPaging]
        public IOrderedQueryable<DbPlayer> Players(Database db, [Parent] DbGame game) {
            return db.Players
                .AsNoTracking()
                .InGame(game.Id)
                .OrderBy(x => x.Id);
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
