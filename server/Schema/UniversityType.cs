namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UniversityType : ObjectType<DbUniversity> {
        protected override void Configure(IObjectTypeDescriptor<DbUniversity> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Universities
                        .SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "University")]
    public class UniversityResolvers {
        public UniversityResolvers(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbUniversityMembership>> Members([Parent] DbUniversity university) {
            return db.UniversityMemberships
                .Include(x => x.Player)
                .Where(x => x.UniversityId == university.Id)
                .ToListAsync();
        }
    }
}
