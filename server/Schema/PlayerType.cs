namespace advisor.Schema
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
        public Task<DbGame> Game(Database db, [Parent] DbPlayer player) {
            return db.Games
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == player.GameId);
        }

        public Task<DbPlayerTurn> LastTurn(Database db, [Parent] DbPlayer player) {
            return db.PlayerTurns
                .AsNoTracking()
                .OnlyPlayer(player)
                .SingleOrDefaultAsync(x => x.TurnNumber == player.LastTurnNumber);
        }

        public Task<DbPlayerTurn> NextTurn(Database db, [Parent] DbPlayer player) {
            return db.PlayerTurns
                .AsNoTracking()
                .Include(x => x.Player)
                .OnlyPlayer(player)
                .SingleOrDefaultAsync(x => x.TurnNumber == player.NextTurnNumber);
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

        public Task<List<DbPlayerTurn>> Turns(Database db, [Parent] DbPlayer player) {
            return db.PlayerTurns
                .AsNoTracking()
                .OnlyPlayer(player)
                .Include(x => x.Player)
                .OrderBy(x => x.TurnNumber)
                .ToListAsync();
        }

        public Task<DbPlayerTurn> Turn(Database db, [Parent] DbPlayer player, int number) {
            return db.PlayerTurns
                .AsNoTracking()
                .OnlyPlayer(player)
                .Include(x => x.Player)
                .SingleOrDefaultAsync(x => x.TurnNumber == number);
        }
    }
}
