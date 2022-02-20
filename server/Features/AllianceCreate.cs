namespace advisor.Features
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record AllianceCreate(long UserId, long PlayerId, string Name) : IRequest<AllianceCreateResult>;

    public record AllianceCreateResult(DbAlliance alliance, bool IsSuccess, string Error) : IMutationResult;

    public class AllianceCreateHandler : IRequestHandler<AllianceCreate, AllianceCreateResult> {
        public AllianceCreateHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<AllianceCreateResult> Handle(AllianceCreate request, CancellationToken cancellationToken) {
            var player = await db.Players
                .Include(x => x.AllianceMembererships)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId && x.UserId == request.UserId);

            if (player == null) {
                return new AllianceCreateResult(null, false, "Faction was not found");
            }

            if (player.AllianceMembererships.Count != 0) {
                return new AllianceCreateResult(null, false, "Faction is member of another alliance");
            }

            var alliance = new DbAlliance {
                GameId = player.GameId,
                Name = request.Name
            };

            alliance.Members.Add(new DbAllianceMember {
                PlayerId = player.Id,
                Owner = true,
                CanInvite = true,
                ShareMap = true,
                TeachMages = true
            });

            await db.Alliances.AddAsync(alliance);
            await db.SaveChangesAsync();

            return new AllianceCreateResult(alliance, true, null);
        }
    }
}
