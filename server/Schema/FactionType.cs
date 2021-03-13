namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class FactionType : ObjectType<DbFaction> {
        protected override void Configure(IObjectTypeDescriptor<DbFaction> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Factions
                        .SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Faction")]
    public class FactionResolvers {
        public FactionResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbEvent>> GetEvents([Parent] DbFaction faction) {
            return db.Events
                .Include(x => x.Faction)
                .Where(x => x.FactionId == faction.Id)
                .ToListAsync();
        }

        public Task<DbUnit> UnitByNumber([Parent] DbStructure structure, int number) {
            return db.Units
                .Include(x => x.Faction)
                .SingleOrDefaultAsync(x => x.StrcutureId == structure.Id && x.Number == number);
        }
    }
}
