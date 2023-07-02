namespace advisor.Schema;

using advisor.Persistence;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

public class GameEngineType : ObjectType<DbGameEngine> {
        protected override void Configure(IObjectTypeDescriptor<DbGameEngine> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.Id)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.GameEngines
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == id);
                });
        }
}
