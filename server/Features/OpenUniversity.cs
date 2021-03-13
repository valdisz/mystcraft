namespace advisor.Features
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record OpenUniversity(long UserId, long PlayerId, string Name) : IRequest<DbUniversity> {

    }

    public class OpenUniversityHandler : IRequestHandler<OpenUniversity, DbUniversity> {
        public OpenUniversityHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbUniversity> Handle(OpenUniversity request, CancellationToken cancellationToken) {
            var player = await db.Players
                .Include(x => x.UniversityMembership)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId);
            if (player == null) return null;

            if (player.UserId != request.UserId) return null;

            // already part of another unversity
            if (player.UniversityMembership != null) return null;

            var membership = new DbUniversityMembership {
                Role = UniveristyMemberRole.Owner
            };

            player.UniversityMembership = membership;

            var university = new DbUniversity {
                GameId = player.GameId,
                Name = request.Name,
                Members = {
                    membership
                }
            };

            await db.Universities.AddAsync(university);
            await db.SaveChangesAsync();

            return university;
        }
    }
}
