namespace atlantis {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using atlantis.Persistence;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;

    public class Query {
        public Query(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public Task<List<DbGame>> GetGames() => db.Games.ToListAsync();
    }

    public class QueryType : ObjectType<Query> {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
        }
    }
}
