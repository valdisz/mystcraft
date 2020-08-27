namespace atlantis {
    using System.Linq;
    using atlantis.Persistence;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;

    public class Query {
        public Query(Database db) {
            this.db = db;
        }

        private readonly Database db;

        [UsePaging]
        public IQueryable<DbGame> Games() => db.Games;
    }

    public class QueryType : ObjectType<Query> {
        protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
        {
        }
    }
}
