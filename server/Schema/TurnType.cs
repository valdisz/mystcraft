// -----------------------------------------------------------------------
//  <copyright file="TurnType.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace atlantis {
    using System.Linq;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class TurnType : ObjectType<DbTurn> {
        protected override void Configure(IObjectTypeDescriptor<DbTurn> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Turns.SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Turn")]
    public class TurnResolvers {
        public TurnResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        [UsePaging]
        public IQueryable<DbReport> GetReports([Parent] DbTurn turn) {
            return db.Reports.Where(x => x.TurnId == turn.Id);
        }

        [UsePaging]
        public IQueryable<DbRegion> GetRegions([Parent] DbTurn turn) {
            return db.Regions.Where(x => x.TurnId == turn.Id);
        }
    }
}
