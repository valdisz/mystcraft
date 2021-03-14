namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record AddStudyPlans(long UniversityId) : IRequest<DbUniversity> {

    }

    public class AddStudyPlansHandler : IRequestHandler<AddStudyPlans, DbUniversity> {
        public AddStudyPlansHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        public async Task<DbUniversity> Handle(AddStudyPlans request, CancellationToken cancellationToken) {
            var university = await db.Universities
                .Include(x => x.Members)
                .ThenInclude(x => x.Player)
                .SingleOrDefaultAsync(x => x.Id == request.UniversityId);

            var latestTurnNumber = await db.Games
                .Include(x => x.Players)
                .ThenInclude(x => x.Turns)
                .Where(x => x.Id == university.GameId)
                .SelectMany(x => x.Players)
                .SelectMany(x => x.Turns)
                .Select(x => x.Number)
                .MaxAsync();

            foreach (var member in university.Members) {
                var playerId = member.PlayerId;
                var factionNumber = member.Player.FactionNumber;

                var units = await db.Units
                    .Include(x => x.Turn)
                    .Include(x => x.Faction)
                    .Include(x => x.Plan)
                    .Where(x => x.Turn.Number == latestTurnNumber
                            && x.Turn.PlayerId == playerId
                            && x.Faction != null
                            && x.Faction.Number == factionNumber
                    )
                    .ToListAsync();

                var mages = units
                    .Where(x => x.Plan == null)
                    .Where(x => x.Skills.Any(s => s.Code == "FORC" || s.Code == "PATT" || s.Code == "SPIR"));

                foreach (var mage in mages) {
                    var plan = new DbStudyPlan {
                        UniversityId = university.Id,
                        TurnId = mage.TurnId,
                        UnitId = mage.Id
                    };

                    await db.StudyPlans.AddAsync(plan);
                }
            }

            await db.SaveChangesAsync();

            return university;
        }
    }
}
