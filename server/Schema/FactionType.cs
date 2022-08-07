namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class FactionType : ObjectType<DbFaction> {
        protected override void Configure(IObjectTypeDescriptor<DbFaction> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompsiteId)
                .ResolveNode((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return DbFaction.FilterById(db.Factions.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }
    }

    [ExtendObjectType("Faction")]
    public class FactionResolvers {
        public Task<List<DbEvent>> Events(Database db, [Parent] DbFaction faction) {
            return db.Events
                .AsNoTracking()
                .OnlyFaction(faction)
                .ToListAsync();
        }
    }
}
