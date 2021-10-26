namespace advisor {
    using System.Linq;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    using UnitId = System.ValueTuple<long, int, int>;

    public class UnitType : ObjectType<DbUnit> {
        protected override void Configure(IObjectTypeDescriptor<DbUnit> descriptor) {
            descriptor.AsNode()
                .IdField(x => MakeId(x))
                .NodeResolver((ctx, id) => {
                    var db = ctx.Service<Database>();
                    return FilterById(db.Units.AsNoTracking(), id).SingleOrDefaultAsync();
                });
        }

        private static UnitId MakeId(DbUnit unit) => (unit.PlayerId, unit.TurnNumber, unit.Number);

        private static IQueryable<DbUnit> FilterById(IQueryable<DbUnit> q, UnitId id) {
            var (playerId, turnNumber, unitNumber) = id;
            return q.Where(x =>
                    x.PlayerId == playerId
                && x.TurnNumber == turnNumber
                && x.Number == unitNumber
            );
        }
    }

    [ExtendObjectType(Name = "Unit")]
    public class UnitResolvers {
        public UnitResolvers(Database db, IIdSerializer idSerializer) {
            this.db = db;
            this.idSerializer = idSerializer;
        }

        private readonly Database db;
        private readonly IIdSerializer idSerializer;

        public string Region([Parent] DbUnit unit) {
            return unit.RegionId;
        }

        public int? Structure([Parent] DbUnit unit) {
            return unit.StrcutureNumber;
        }
    }
}
