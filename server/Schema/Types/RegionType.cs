namespace advisor.Schema {
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class RegionType : ObjectType<DbRegion> {
        protected override void Configure(IObjectTypeDescriptor<DbRegion> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.PublicId)
                .ResolveNode(async (ctx, idValue) => {
                    if (!await ctx.AuthorizeAsync(Policies.OwnPlayer)) {
                        return null;
                    }

                    var id = RegionId.CreateFrom(idValue);

                    var db = ctx.Service<Database>();
                    return await db.Regions
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
