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

    public class PlayerType : ObjectType<DbPlayer> {
        protected override void Configure(IObjectTypeDescriptor<DbPlayer> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) =>
                {
                    var db = ctx.Service<Database>();
                    return db.Players
                        .Include(x => x.Game)
                        .Include(x => x.UniversityMembership)
                        .ThenInclude(x => x.University)
                        .FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    public class PlayerUniversity {
        public PlayerUniversity(DbUniversity University, UniveristyMemberRole Role) {
            this.University = University;
            this.Role = Role;
        }

        public DbUniversity University { get; }
        public UniveristyMemberRole Role { get; }
    }

    [ExtendObjectType(Name = "Player")]
    public class PlayerResolvers {
        public PlayerResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<DbGame> Game([Parent] DbPlayer player) {
            return db.Games.SingleOrDefaultAsync(x => x.Id == player.GameId);
        }

        public async Task<PlayerUniversity> University([Parent] DbPlayer player) {
            var membership = await db.UniversityMemberships
                .Include(x => x.University)
                .SingleOrDefaultAsync(x => x.PlayerId == player.Id);

            return membership == null
                ? null
                : new PlayerUniversity(membership.University, membership.Role);
        }

        public Task<List<DbReport>> Reports([Parent] DbPlayer player, long? turn = null) {
            var q = db.Reports
                .Where(x => x.PlayerId == player.Id);

            if (turn != null) {
                q = q
                    .Include(x => x.Turn)
                    .Where(x => x.Turn.Number == turn);
            }

            return q.ToListAsync();
        }

        public Task<List<DbTurn>> Turns([Parent] DbPlayer player) {
            return db.Turns
                .Where(x => x.PlayerId == player.Id)
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public Task<DbTurn> TurnByNumber([Parent] DbPlayer player, long turn) {
            return db.Turns
                .FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.Number == turn);
        }
    }
}
