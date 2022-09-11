namespace advisor.Schema {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using GreenDonut;
    using HotChocolate;
    using HotChocolate.Types;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UnitType : ObjectType<DbUnit> {
        protected override void Configure(IObjectTypeDescriptor<DbUnit> descriptor) {
            descriptor
                .ImplementsNode()
                .IdField(x => x.CompositeId)
                .ResolveNode((ctx, id) => {
                    var parsedId = DbUnit.ParseId(id);
                    var db = ctx.Service<Database>();

                    var requestedFields = ctx.GetSelections(this);
                    bool includePlan = requestedFields.Any(x => x.Field.Name == nameof(DbUnit.StudyPlan));

                    var query = DbUnit.FilterById(db.Units.AsNoTracking(), parsedId);
                    if (includePlan) {
                        query = query.Include(x => x.StudyPlan);
                    }

                    return query.SingleOrDefaultAsync();
                });
        }
    }

    [ExtendObjectType("Unit")]
    public class UnitResolvers {
    }

    // public class UnitBatchDataLoader : BatchDataLoader<string, DbUnit>
    // {
    //     protected override Task<IReadOnlyDictionary<string, DbUnit>> LoadBatchAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken)
    //     {
    //         throw new System.NotImplementedException();
    //     }
    // }
}
