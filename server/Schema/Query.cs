namespace advisor {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using HotChocolate.AspNetCore.Authorization;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;

    [Authorize]
    public class Query {
        public Task<List<DbGame>> Games(Database db) => db.Games.ToListAsync();

        [Authorize(Policy = Policies.UserManagers)]
        [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 1000)]
        public IQueryable<DbUser> Users(Database db) {
            return db.Users;
        }

        [Authorize]
        public async Task<DbUser> Me(Database db, [GlobalState] long currentUserId) {
            return await db.Users.FindAsync(currentUserId);
        }
    }

    public class QueryType : ObjectType<Query> {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor) {
        }
    }
}
