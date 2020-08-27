// -----------------------------------------------------------------------
//  <copyright file="GameType.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace atlantis {
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class GameType : ObjectType<DbGame> {
        protected override void Configure(IObjectTypeDescriptor<DbGame> descriptor)
        {
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

        [UsePaging]
        public IQueryable<DbReport> GetReports([Parent] DbGame game, long? turn = null) {
            var q = db.Reports
                .Where(x => x.GameId == game.Id);

            if (turn != null) {
                q = q
                    .Include(x => x.Turn)
                    .Where(x => x.Turn.Number == turn);
            }

            return q;
        }

        [UsePaging]
        public IQueryable<DbTurn> GetTurns([Parent] DbGame game) {
            return db.Turns
                .Where(x => x.GameId == game.Id)
                .OrderBy(x => x.Number);
        }

        public Task<DbTurn> GetTurn([Parent] DbGame game, long turn) {
            return db.Turns
                .FirstOrDefaultAsync(x => x.GameId == game.Id && x.Number == turn);
        }
    }
}
