namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record SetupStudyPlans(long PlayerId) : IRequest<DbUniversity> {

    }

    public class SetupStudyPlansHandler : IRequestHandler<SetupStudyPlans, DbUniversity> {
        public SetupStudyPlansHandler(Database db, IMapper mapper) {
            this.db = db;
            this.mapper = mapper;
        }

        private readonly Database db;
        private readonly IMapper mapper;

        public async Task<DbUniversity> Handle(SetupStudyPlans request, CancellationToken cancellationToken) {
            var player = await db.Players
                .Include(x => x.UniversityMembership)
                .SingleOrDefaultAsync(x => x.Id == request.PlayerId);

            if (player.UniversityMembership == null) return null;

            var university = await db.Universities
                .Include(x => x.Members)
                .ThenInclude(x => x.Player)
                .Include(x => x.Plans)
                .ThenInclude(x => x.Unit)
                .SingleOrDefaultAsync(x => x.Id == player.UniversityMembership.UniversityId);

            var latestTurnNumber = await db.Games
                .Include(x => x.Players)
                .ThenInclude(x => x.Turns)
                .Where(x => x.Id == university.GameId)
                .SelectMany(x => x.Players)
                .SelectMany(x => x.Turns)
                .Select(x => x.Number)
                .MaxAsync();

            var plans = university.Plans
                .GroupBy(x => x.Unit.Number)
                .ToDictionary(x => x.Key, x => x.OrderBy(p => p.Id).Last());

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

                    if (plans.ContainsKey(mage.Number)) {
                        plan.Target = mapper.Map<DbSkill>(plans[mage.Number]);
                    }

                    await db.StudyPlans.AddAsync(plan);
                }
            }

            await db.SaveChangesAsync();

            return university;
        }
    }
}
