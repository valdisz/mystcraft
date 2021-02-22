// -----------------------------------------------------------------------
//  <copyright file="TurnType.cs" company="Akka.NET Project">
//      Copyright (C) 2009-2020 Lightbend Inc. <http://www.lightbend.com>
//      Copyright (C) 2013-2020 .NET Foundation <https://github.com/akkadotnet/akka.net>
//  </copyright>
// -----------------------------------------------------------------------

namespace atlantis
{
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UnitType : ObjectType<DbUnit> {
        protected override void Configure(IObjectTypeDescriptor<DbUnit> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Units.SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }
}
