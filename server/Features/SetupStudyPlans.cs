namespace advisor.Features {
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record SetupStudyPlans(long PlayerId) : IRequest {

    }

    public class SetupStudyPlansHandler : IRequestHandler<SetupStudyPlans> {
        public SetupStudyPlansHandler(Database db, IMapper mapper) {
            this.db = db;
            this.mapper = mapper;
        }

        private readonly Database db;
        private readonly IMapper mapper;

        public async Task<Unit> Handle(SetupStudyPlans request, CancellationToken cancellationToken) {
            var membership = await db.UniversityMemberships
                .Include(x => x.Player)
                .SingleOrDefaultAsync(x => x.PlayerId == request.PlayerId);

            if (membership == null) return Unit.Value;

            var universityId = membership.UniversityId;
            var factionNumber = membership.Player.FactionNumber;
            var turnNumber = membership.Player.LastTurnNumber;

            async Task<Dictionary<int, DbStudyPlan>> getTurnPlans(int tnum) {
                return (await db.StudyPlans
                    .Include(x => x.Turn)
                    .Include(x => x.Unit)
                    .ThenInclude(x => x.Faction)
                    .Where(x => x.UniversityId == membership.UniversityId
                        && x.Turn.Number == turnNumber
                        && x.Turn.PlayerId == request.PlayerId
                        && x.Unit.Faction != null
                        && x.Unit.Faction.Number == factionNumber)
                    .ToListAsync())
                    .ToDictionary(x => x.Unit.Number);
            }

            var plans = await getTurnPlans(turnNumber);
            var prevPalns = await getTurnPlans(turnNumber - 1);

            var mages = await db.Units
                .Include(x => x.Turn)
                .Include(x => x.Faction)
                .Include(x => x.Plan)
                .Where(x => x.Turn.Number == turnNumber
                        && x.Turn.PlayerId == request.PlayerId
                        && x.Faction != null
                        && x.Faction.Number == factionNumber
                        && x.Skills.Any(s => s.Code == "FORC" || s.Code == "PATT" || s.Code == "SPIR")
                )
                .ToListAsync();

            foreach (var mage in mages) {
                if (plans.ContainsKey(mage.Number)) continue;

                var plan = new DbStudyPlan {
                    UniversityId = universityId,
                    TurnId = mage.TurnId,
                    UnitId = mage.Id
                };

                if (prevPalns.ContainsKey(mage.Number)) {
                    plan.Target = mapper.Map<DbSkill>(prevPalns[mage.Number]);
                }

                await db.StudyPlans.AddAsync(plan);
            }

            await db.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
