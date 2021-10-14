namespace advisor.Features
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record OpenUniversity(long UserId, long PlayerId, string Name) : IRequest<DbAlliance> {

    }

    public class OpenUniversityHandler : IRequestHandler<OpenUniversity, DbAlliance> {
        public OpenUniversityHandler(Database db, IMediator mediator) {
            this.db = db;
            this.mediator = mediator;
        }

        private readonly Database db;
        private readonly IMediator mediator;

        public async Task<DbAlliance> Handle(OpenUniversity request, CancellationToken cancellationToken) {
            var player = await db.Players
                .Include(x => x.UniversityMembership)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId);
            if (player == null) return null;

            if (player.UserId != request.UserId) return null;

            // already part of another unversity
            if (player.UniversityMembership != null) return null;

            var membership = new DbAllianceMember {
                Role = AllianceMemberRole.Owner,
                PlayerId = request.PlayerId
            };

            player.UniversityMembership = membership;

            var university = new DbAlliance {
                GameId = player.GameId,
                Name = request.Name,
                Members = {
                    membership
                }
            };

            await db.Universities.AddAsync(university);
            await db.SaveChangesAsync();

            await mediator.Send(new SetupStudyPlans(request.PlayerId));

            return university;
        }
    }
}
