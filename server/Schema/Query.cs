namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    public class Query {
        public Query(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbGame>> Games() => db.Games.ToListAsync();

        [Authorize(Policy = Policies.UserManagers)]
        [UsePaging]
        public IQueryable<DbUser> Users() {
            return db.Users;
        }

        [Authorize]
        public async Task<DbUser> Me([GlobalState] long currentUserId) {
            return await db.Users.FindAsync(currentUserId);
        }
    }

    public class QueryType : ObjectType<Query> {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
        }
    }
}
