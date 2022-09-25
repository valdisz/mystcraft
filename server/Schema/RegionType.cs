namespace advisor.Schema {
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class RegionType : ObjectType<DbRegion> {
        protected override void Configure(IObjectTypeDescriptor<DbRegion> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.PublicId)
                .ResolveNode((ctx, idValue) => {
                    var id = RegionId.CreateFrom(idValue);

                    var db = ctx.Service<Database>();
                    return db.Regions
                        .AsNoTracking()
                        .OnlyPlayer(id.PlayerId)
                        .SingleOrDefaultAsync(x => x.TurnNumber == id.TurnNumber && x.X == id.X && x.Y == id.Y && x.Z == id.Z);
                });
        }
    }

    [ExtendObjectType("Region")]
    public class RegionResolvers {
    }
}
