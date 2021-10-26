namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Features;
    using HotChocolate;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Persistence;

    public class GameType : ObjectType<DbGame> {
        protected override void Configure(IObjectTypeDescriptor<DbGame> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) =>
                {
                    var db = ctx.Service<Database>();
                    return db.Games
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Game")]
    public class GameResolvers {
        public GameResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public GameOptions Options([Parent] DbGame game) {
            return game.Options != null
                ? JsonConvert.DeserializeObject<GameOptions>(game.Options)
                : null;
        }

        public Task<DbPlayer> Me([Parent] DbGame game, [GlobalState] long currentUserId) {
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
        public Task<List<DbPlayer>> Players([Parent] DbGame game) {
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
