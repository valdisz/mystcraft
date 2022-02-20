namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record AllianceJoin(long UserId, long PlayerId, long AllianceId) : IRequest<AllianceJoinResult>;

    public record AllianceJoinResult(DbAlliance alliance, DbAllianceMember membership, bool IsSuccess, string Error) : IMutationResult;

    public class AllianceJoinHandler : IRequestHandler<AllianceJoin, AllianceJoinResult> {
        public AllianceJoinHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<AllianceJoinResult> Handle(AllianceJoin request, CancellationToken cancellationToken) {
            var player = await db.Players
                .Include(x => x.AllianceMembererships)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

            if (player == null) {
                return new AllianceJoinResult(null, null, false, "Faction was not found");
            }

            var alliance = await db.Alliances
                .SingleOrDefaultAsync(x => x.Id == request.AllianceId);
            if (alliance == null) {
                return new AllianceJoinResult(null, null, false, "Alliance was not found");
            }

            var membership = player.AllianceMembererships.SingleOrDefault();
            if (membership == null) {
                membership = new DbAllianceMember {
                    PlayerId = player.Id,
                    Owner = false,
                    CanInvite = false,
                    ShareMap = true,
                    TeachMages = true
                };

                alliance.Members.Add(membership);
                await db.AllianceMembers.AddAsync(membership);
            }

            membership.AllianceId = alliance.Id;

            await db.SaveChangesAsync();

            return new AllianceJoinResult(alliance, membership, true, null);
        }
    }
}
