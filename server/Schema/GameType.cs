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
        public GameOptions Options(Database db, [Parent] DbGame game) {
            return game.Options != null
                ? JsonConvert.DeserializeObject<GameOptions>(game.Options)
                : null;
        }

        public Task<DbPlayer> Me(Database db, [Parent] DbGame game, [GlobalState] long currentUserId) {
            return db.Players
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.GameId == game.Id && x.UserId == currentUserId);
        }

        // public Task<DbAlliance> MyUniversity([Parent] DbGame game, [GlobalState] long currentUserId) {
            // return db.Players
            //     .Include(x => x.UniversityMembership)
            //     .ThenInclude(x => x.University)
            //     .Where(x => x.GameId == game.Id && x.UserId == currentUserId && x.UniversityMembership != null)
            //     .Select(x => x.UniversityMembership.University)
            //     .SingleOrDefaultAsync();
        // }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<List<DbPlayer>> Players(Database db, [Parent] DbGame game) {
            return db.Players
                .AsNoTracking()
                .Where(x => x.GameId == game.Id).ToListAsync();
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
