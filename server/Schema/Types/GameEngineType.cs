namespace advisor.Schema;

using advisor.Persistence;
using HotChocolate.Types;
using Microsoft.EntityFrameworkCore;

public class GameEngineType : ObjectType<DbGameEngine> {
    public const string NAME = "GameEngine";

    protected override void Configure(IObjectTypeDescriptor<DbGameEngine> descriptor) {
        descriptor.Name(NAME);

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
