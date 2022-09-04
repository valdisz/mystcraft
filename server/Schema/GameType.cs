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
    using Remote;

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

    public record RemotePlayer(int Number, string Name, bool Orders, bool Times);

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

        public async Task<IOrderedQueryable<RemotePlayer>> RemotePlayers(Database db, [Service] NewOriginsClient client, [Parent] DbGame game) {
            if (game.Options == null) {
                game.Options = await db.Games.Where(x => x.Id == game.Id).Select(x => x.Options).SingleOrDefaultAsync();
            }

            var factions = (await client.ListFactionsAsync())
                .Where(x => x.Number.HasValue)
                .Select(x => new RemotePlayer(x.Number!.Value, x.Name, x.OrdersSubmitted, x.TimesSubmitted));
            return factions.AsQueryable().OrderBy(x => x.Number);
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
