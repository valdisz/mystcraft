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
                .ResolveNode((ctx, id) => {
                    var (gameId, turnNumber) = TurnId.CreateFrom(id);
                    var db = ctx.Service<Database>();
                    return db.Turns
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
