namespace advisor {
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
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Players
                        .AsNoTracking()
                        .Include(x => x.Game)
                        // .Include(x => x.UniversityMembership)
                        // .ThenInclude(x => x.University)
                        .FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    // public class PlayerUniversity {
    //     public PlayerUniversity(DbAlliance University, AllianceMemberRole Role) {
    //         this.University = University;
    //         this.Role = Role;
    //     }

    //     public DbAlliance University { get; }
    //     public AllianceMemberRole Role { get; }
    // }

    [ExtendObjectType(Name = "Player")]
    public class PlayerResolvers {
        public PlayerResolvers(Database db, IIdSerializer idSerializer) {
            this.db = db;
            this.idSerializer = idSerializer;
        }

        private readonly Database db;
        private readonly IIdSerializer idSerializer;

        public Task<DbGame> Game([Parent] DbPlayer player) {
            return db.Games
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == player.GameId);
        }

        public string LastTurnId([Parent] DbPlayer player) {
            return idSerializer.Serialize("Turn", TurnType.MakeId(player.Id, player.LastTurnNumber));
        }

        // public async Task<PlayerUniversity> University([Parent] DbPlayer player) {
        //     var membership = await db.UniversityMemberships
        //         .Include(x => x.University)
        //         .SingleOrDefaultAsync(x => x.PlayerId == player.Id);

        //     return membership == null
        //         ? null
        //         : new PlayerUniversity(membership.University, membership.Role);
        // }

        public Task<List<DbReport>> Reports([Parent] DbPlayer player, int? turn = null) {
            var q = db.Reports
                .AsNoTracking()
                .FilterByPlayer(player);

            if (turn != null) {
                q = q.Where(x => x.TurnNumber == turn);
            }

            return q.ToListAsync();
        }

        public Task<List<DbTurn>> Turns([Parent] DbPlayer player) {
            return db.Turns
                .AsNoTracking()
                .FilterByPlayer(player)
                .OrderBy(x => x.Number)
                .ToListAsync();
        }

        public Task<DbTurn> Turn([Parent] DbPlayer player, int number) {
            return db.Turns
                .AsNoTracking()
                .FilterByPlayer(player)
                .SingleOrDefaultAsync(x => x.Number == number);
        }

        // public async Task<FactionStats> Stats([Parent] DbPlayer player) {
        //     var stats = await db.Stats
        //         .AsNoTracking()
        //         .FilterByPlayer(player)
        //         .Where(x => x.FactionNumber == player.Number)
        //         .ToListAsync();

        //     DbIncomeStats income = new DbIncomeStats();
        //     Dictionary<string, int> production = new Dictionary<string, int>();

        //     foreach (var stat in stats) {
        //         income.Pillage += stat.Income.Pillage;
        //         income.Tax += stat.Income.Tax;
        //         income.Trade += stat.Income.Trade;
        //         income.Work += stat.Income.Work;

        //         foreach (var item in stat.Production) {
        //             production[item.Code] = production.TryGetValue(item.Code, out var value)
        //                 ? value + item.Amount
        //                 : item.Amount;
        //         }
        //     }

        //     return new FactionStats {
        //         Income = income,
        //         Production = production.Select(x => new DbItem { Code = x.Key, Amount = x.Value }).ToList()
        //     };
        // }
    }
}
