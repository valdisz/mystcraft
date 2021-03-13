namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UserType : ObjectType<DbUser> {
        protected override void Configure(IObjectTypeDescriptor<DbUser> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return db.Users
                        .SingleOrDefaultAsync(x => x.Id == id);
                });
        }
    }

    [ExtendObjectType(Name = "User")]
    public class UserResolvers {
        public UserResolvers(Database db) {
        }

        private readonly Database db;

        public Task<List<DbUserGame>> GetGames([Parent] DbUser user) {
            return db.UserGames
                .Where(x => x.UserId == user.Id)
                .ToListAsync();
        }
    }
}
