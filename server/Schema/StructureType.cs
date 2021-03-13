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

    public class StructureType : ObjectType<DbStructure> {
        protected override void Configure(IObjectTypeDescriptor<DbStructure> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Structures.FirstOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "Structure")]
    public class StructureResolvers {
        public StructureResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbUnit>> GetUnits([Parent] DbStructure structure) {
            return db.Units
                .Include(x => x.Faction)
                .Where(x => x.StrcutureId == structure.Id)
                .ToListAsync();
        }

        public Task<DbUnit> UnitByNumber([Parent] DbStructure structure, int number) {
            return db.Units
                .Include(x => x.Faction)
                .SingleOrDefaultAsync(x => x.StrcutureId == structure.Id && x.Number == number);
        }
    }
}
