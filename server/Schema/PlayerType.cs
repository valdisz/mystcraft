namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Resolvers;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class PlayerType : ObjectType<DbPlayer> {
        protected override void Configure(IObjectTypeDescriptor<DbPlayer> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Players
                        .AsNoTracking()
                        .Include(x => x.Game)
                        .FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType("Player")]
    public class PlayerResolvers {
        public PlayerResolvers(IIdSerializer idSerializer) {
            this.idSerializer = idSerializer;
        }

        private readonly IIdSerializer idSerializer;

        public Task<DbGame> Game(Database db, [Parent] DbPlayer player) {
            return db.Games
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == player.GameId);
        }

        public string LastTurnId(IResolverContext context, [Parent] DbPlayer player) {
            var id = idSerializer.Serialize(null, "Turn", DbTurn.MakeId(player.Id, player.LastTurnNumber));
            return id;
        }

        public Task<DbAlliance> Alliance(Database db, [Parent] DbPlayer player) {
            return db.Alliances
                .Include(x => x.Members)
                .ThenInclude(x => x.Player)
                .Where(x => x.GameId == player.GameId && x.Members.Any(m => m.PlayerId == player.Id))
                .SingleOrDefaultAsync();
        }

        public Task<List<DbReport>> Reports(Database db, [Parent] DbPlayer player, int? turn = null) {
            var q = db.Reports
                .AsNoTracking()
                .FilterByPlayer(player);

            if (turn != null) {
                q = q.Where(x => x.TurnNumber == turn);
            }

            return q.ToListAsync();
        }

        public Task<List<DbTurn>> Turns(Database db, [Parent] DbPlayer player) {
            return db.Turns
                .AsNoTracking()
                .Include(x => x.Player)
                .FilterByPlayer(player)
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public Task<DbTurn> Turn(Database db, [Parent] DbPlayer player, int number) {
            return db.Turns
                .AsNoTracking()
                .Include(x => x.Player)
                .FilterByPlayer(player)
                .SingleOrDefaultAsync(x => x.Number == number);
        }
    }
}
