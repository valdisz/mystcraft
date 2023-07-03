// FIXME
namespace advisor.Features;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using advisor.Schema;
using advisor.Persistence;

public record AllianceInvite(long AllianceId, long PlayerId, long UserId, long TargetUserId): IRequest<AllianceInviteResult>;

public record AllianceInviteResult(bool IsSuccess, string Error = null, DbAlliance Alliance = null, DbAllianceMember Membership = null) : IMutationResult;

// public class AllianceInviteHandler : IRequestHandler<AllianceInvite, AllianceInviteResult> {
//     public AllianceInviteHandler(IUnitOfWork unit) {
//         this.unit = unit;
//     }

//     private readonly IUnitOfWork unit;

//     public async Task<AllianceInviteResult> Handle(AllianceInvite request, CancellationToken cancellationToken) {
//         throw new NotImplementedException();
//     }
// }
