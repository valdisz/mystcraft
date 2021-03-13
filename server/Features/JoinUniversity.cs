namespace advisor.Features
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record JoinUniversity(long UserId, long PlayerId, long UniversityId) : IRequest<DbUniversity> {

    }

    public class JoinUniversityHandler : IRequestHandler<JoinUniversity, DbUniversity> {
        public JoinUniversityHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbUniversity> Handle(JoinUniversity request, CancellationToken cancellationToken) {
            var player = await db.Players
                .Include(x => x.UniversityMembership)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

            // must close university first
            if (player.UniversityMembership?.Role == UniveristyMemberRole.Owner) return null;

            var university = await db.Universities
                .Include(x => x.Members)
                .SingleOrDefaultAsync(x => x.Id == request.UniversityId);
            if (university == null) return null;

            // must be from the same game
            if (player.GameId != university.GameId) return null;

            if (player?.UniversityMembership?.UniversityId == university.Id) return university;

            // can participate only in one university
            if (player.UniversityMembership != null) {
                db.Remove(player.UniversityMembership);
                university.Members.Remove(player.UniversityMembership);
            }

            var membership = new DbUniversityMembership {
                PlayerId = player.Id,
                UniversityId = university.Id,
                Role = UniveristyMemberRole.Member
            };

            player.UniversityMembership = membership;
            university.Members.Add(membership);

            await db.SaveChangesAsync();

            return university;
        }
    }
}
