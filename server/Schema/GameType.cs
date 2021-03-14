namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.AspNetCore.Authorization;
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

        public Task<DbPlayer> MyPlayer([Parent] DbGame game, [GlobalState] long currentUserId) {
            return db.Players
                .SingleOrDefaultAsync(x => x.GameId == game.Id && x.UserId == currentUserId);
        }

        public Task<DbUniversity> MyUniversity([Parent] DbGame game, [GlobalState] long currentUserId) {
            return db.Players
                .Include(x => x.UniversityMembership)
                .ThenInclude(x => x.University)
                .Where(x => x.GameId == game.Id && x.UserId == currentUserId && x.UniversityMembership != null)
                .Select(x => x.UniversityMembership.University)
                .SingleOrDefaultAsync();
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<List<DbPlayer>> Players([Parent] DbGame game) {
            return db.Players.Where(x => x.GameId == game.Id).ToListAsync();
        }

        [Authorize(Policy = Policies.GameMasters)]
        public Task<List<DbUniversity>> Universities([Parent] DbGame game) {
            return db.Universities
                .Include(x => x.Members)
                .ThenInclude(x => x.Player)
                .Where(x => x.GameId == game.Id).ToListAsync();
        }
    }
}
