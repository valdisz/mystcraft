namespace advisor.Schema
{
    using System.Linq;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class TurnType : ObjectType<DbTurn> {
        protected override void Configure(IObjectTypeDescriptor<DbTurn> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.PublicId)
                .ResolveNode(async (ctx, id) => {
                    if (!await ctx.AuthorizeAsync(Policies.GameMasters)) {
                        return null;
                    }

                    var (gameId, turnNumber) = TurnId.CreateFrom(id);
                    var db = ctx.Service<Database>();
                    return await db.Turns
                        .AsNoTracking()
                        .InGame(gameId)
                        .SingleOrDefaultAsync(x => x.Number == turnNumber);
                });
        }
    }

    [ExtendObjectType("Turn")]
    public class TurnResolvers {
        public IOrderedQueryable<DbReport> Reports(Database db, [Parent] DbTurn turn) {
            return db.Reports
                .AsNoTracking()
                .InGame(turn.GameId)
                .Where(x => x.TurnNumber == turn.Number)
                .OrderBy(x => x.FactionNumber);
        }
    }
}
