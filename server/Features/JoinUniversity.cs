namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;

    public record JoinUniversity(long UserId, long PlayerId, long UniversityId) : IRequest<DbAlliance>;

    public class JoinUniversityHandler : IRequestHandler<JoinUniversity, DbAlliance> {
        public JoinUniversityHandler(Database db, IMediator mediator) {
            this.db = db;
            this.mediator = mediator;
        }

        private readonly Database db;
        private readonly IMediator mediator;

        public async Task<DbAlliance> Handle(JoinUniversity request, CancellationToken cancellationToken) {
            // var player = await db.Players
            //     .Include(x => x.UniversityMembership)
            //     .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

            // // must close university first
            // if (player.UniversityMembership?.Role == AllianceMemberRole.Owner) return null;

            // var university = await db.Universities
            //     .Include(x => x.Members)
            //     .SingleOrDefaultAsync(x => x.Id == request.UniversityId);
            // if (university == null) return null;

            // // must be from the same game
            // if (player.GameId != university.GameId) return null;

            // if (player?.UniversityMembership?.UniversityId == university.Id) return university;

            // // can participate only in one university
            // if (player.UniversityMembership != null) {
            //     db.Remove(player.UniversityMembership);
            //     university.Members.Remove(player.UniversityMembership);
            // }

            // var membership = new DbAllianceMember {
            //     PlayerId = player.Id,
            //     UniversityId = university.Id,
            //     Role = AllianceMemberRole.Member
            // };

            // player.UniversityMembership = membership;
            // university.Members.Add(membership);

            // await db.SaveChangesAsync();

            // await mediator.Send(new SetupStudyPlans(university.Id));

            // return university;
            return null;
        }
    }
}
