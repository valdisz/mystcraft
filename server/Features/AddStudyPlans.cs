// namespace advisor.Features {
//     using System.Linq;
//     using System.Threading;
//     using System.Threading.Tasks;
//     using advisor.Persistence;
//     using MediatR;

//     public record AddStudyPlans(long UniversityId, long UserId) : IRequest<DbUniversity> {

//     }

//     public class AddStudyPlansHandler : IRequestHandler<AddStudyPlans, DbUniversity> {
//         public AddStudyPlansHandler(Database db) {
//             this.db = db;
//         }

//         private readonly Database db;

//         public async Task<DbUniversity> Handle(AddStudyPlans request, CancellationToken cancellationToken) {
//             var university = await db.Universities.FindAsync(request.UniversityId);

//             var latestTurn = db.Players.
//             db.Units
//                 .Where(x => x.TurnId == request.TurnId)
//         }
//     }
// }
