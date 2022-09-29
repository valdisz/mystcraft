namespace advisor.Schema
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Data;
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
        public Task<DbGame> Game(Database db, [Parent] DbPlayer player) {
            return db.Games
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == player.GameId);
        }

        [UseSingleOrDefault]
        public IQueryable<DbPlayerTurn> LastTurn(Database db, [Parent] DbPlayer player) {
            return db.PlayerTurns
                .AsNoTracking()
                .OnlyPlayer(player)
                .Where(x => x.TurnNumber == player.LastTurnNumber);
        }

        [UseSingleOrDefault]
        public IQueryable<DbPlayerTurn> NextTurn(Database db, [Parent] DbPlayer player) {
            return db.PlayerTurns
                .AsNoTracking()
                .OnlyPlayer(player)
                .Where(x => x.TurnNumber == player.NextTurnNumber);
        }

        public Task<DbAlliance> Alliance(Database db, [Parent] DbPlayer player) {
            return db.Alliances
                .Include(x => x.Members)
                .ThenInclude(x => x.Player)
                .Where(x => x.GameId == player.GameId && x.Members.Any(m => m.PlayerId == player.Id))
                .SingleOrDefaultAsync();
        }

        // public IQueryable<DbReport> Reports(Database db, [Parent] DbPlayer player, int? turn = null) {
        //     var q = db.Reports
        //         .AsNoTracking()
        //         .OnlyPlayer(player);

        //     if (turn != null) {
        //         q = q.Where(x => x.TurnNumber == turn);
        //     }

        //     return q;
        // }

        [UseOffsetPaging]
        public IQueryable<DbPlayerTurn> Turns(Database db, [Parent] DbPlayer player) {
            return db.PlayerTurns
                .AsNoTracking()
                .OnlyPlayer(player)
                .OrderByDescending(x => x.TurnNumber);
        }

        [UseSingleOrDefault]
        public IQueryable<DbPlayerTurn> Turn(Database db, [Parent] DbPlayer player, int number) {
            return db.PlayerTurns
                .AsNoTracking()
                .OnlyPlayer(player)
                .Where(x => x.TurnNumber == number);
        }
    }
}
