namespace advisor {
    using System.Linq;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    using StructureId = System.ValueTuple<long, int, string>;

    public class StructureType : ObjectType<DbStructure> {
        protected override void Configure(IObjectTypeDescriptor<DbStructure> descriptor) {
            descriptor.AsNode()
                .IdField(x => MakeId(x))
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return FilterById(db.Structures.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }

        private static StructureId MakeId(long playerId, int turnNumber, string structureId) => (playerId, turnNumber, structureId);
        private static StructureId MakeId(DbStructure structure) => (structure.PlayerId, structure.TurnNumber, structure.Id);

        private static IQueryable<DbStructure> FilterById(IQueryable<DbStructure> q, StructureId id) {
            var (playerId, turnNumber, structureId) = id;
            return q.Where(x =>
                    x.PlayerId == playerId
                && x.TurnNumber == turnNumber
                && x.Id == structureId
            );
        }
    }

    [ExtendObjectType(Name = "Structure")]
    public class StructureResolvers {
        public StructureResolvers(Database db, IIdSerializer idSerializer) {
            this.db = db;
            this.idSerializer = idSerializer;
        }

        private readonly Database db;
        private readonly IIdSerializer idSerializer;

        public string Region([Parent] DbStructure structure) {
            return structure.RegionId;
        }
    }
}
