// FIXME
namespace advisor.Features;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using advisor.Persistence;
using advisor.Schema;
using MediatR;
using Microsoft.EntityFrameworkCore;

public record AllianceJoin(long UserId, long PlayerId, long AllianceId) : IRequest<AllianceJoinResult>;

public record AllianceJoinResult(bool IsSuccess, string Error = null, DbAlliance Alliance = null, DbAllianceMember Membership = null) : IMutationResult;

// public class AllianceJoinHandler : IRequestHandler<AllianceJoin, AllianceJoinResult> {
//     public AllianceJoinHandler(Database db) {
//         this.db = db;
//     }

//     private readonly Database db;

//     public async Task<AllianceJoinResult> Handle(AllianceJoin request, CancellationToken cancellationToken) {
//         var player = await db.Players
//             .OnlyActivePlayers()
//             .Include(x => x.AllianceMembererships)
//             .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

//         if (player == null) {
//             return new AllianceJoinResult(false, "Faction was not found");
//         }

//         var alliance = await db.Alliances
//             .Include(x => x.Members)
//             .SingleOrDefaultAsync(x => x.Id == request.AllianceId);

//         if (alliance == null) {
//             return new AllianceJoinResult(false, "Alliance was not found");
//         }

//         var membership = player.AllianceMembererships.SingleOrDefault();
//         if (membership == null) {
//             membership = new DbAllianceMember {
//                 PlayerId = player.Id,
//                 Owner = false,
//                 CanInvite = false,
//                 ShareMap = true,
//                 TeachMages = true
//             };

//             alliance.Members.Add(membership);
//             await db.AllianceMembers.AddAsync(membership);
//         }

//         membership.AllianceId = alliance.Id;

//         await db.SaveChangesAsync();

//         var allies = alliance.Members.Select(x => x.PlayerId).ToList();
//         if (!allies.Contains(player.Id)) {
//             allies.Add(player.Id);
//         }

//         // // add reports of other
//         // var lastReport = await GetLastReportAsync(player.Id, player.Number.Value, player.LastTurnNumber);

//         return new AllianceJoinResult(true, Alliance: alliance, Membership: membership);
//     }

//     // private Task<DbReport> GetLastReportAsync(long playerId, int factionNumber, int turnNumber) {
//     //     return db.Reports
//     //         .FirstOrDefaultAsync(x => x.PlayerId == playerId && x.FactionNumber == x.FactionNumber && x.TurnNumber == turnNumber);
//     // }
// }
