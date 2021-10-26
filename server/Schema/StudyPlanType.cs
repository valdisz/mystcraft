// namespace advisor
// {
//     using HotChocolate.Types;
//     using HotChocolate.Types.Relay;
//     using Microsoft.EntityFrameworkCore;
//     using Persistence;

//     public class StudyPlanType : ObjectType<DbStudyPlan> {
//         protected override void Configure(IObjectTypeDescriptor<DbStudyPlan> descriptor) {
//             descriptor.AsNode()
//                 .IdField(x => x.Id)
//                 .NodeResolver((ctx, id) => {
//                     var db = ctx.Service<Database>();
//                     return db.StudyPlans
//                         .Include(x => x.Unit)
//                         .SingleOrDefaultAsync(x => x.Id == id);
//                 });
//         }
//     }
// }
