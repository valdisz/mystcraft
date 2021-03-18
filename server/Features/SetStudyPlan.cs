namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record SetStudyPlanTarget(long UserId, long StudyPlanId, string Skill, int Level) : IRequest<DbStudyPlan> {
    }

    public record SetStudPlanyStudy(long UserId, long StudyPlanId, string Skill) : IRequest<DbStudyPlan> {

    }

    public record SetStudyPlanTeach(long UserId, long StudyPlanId, long[] Units) : IRequest<DbStudyPlan> {

    }

    public class SetStudyPlanTargetHandler :
        IRequestHandler<SetStudyPlanTarget, DbStudyPlan>,
        IRequestHandler<SetStudPlanyStudy, DbStudyPlan>,
        IRequestHandler<SetStudyPlanTeach, DbStudyPlan> {
        public SetStudyPlanTargetHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        private Task<DbStudyPlan> GetPlan(long studyPlanId) {
            return db.StudyPlans
                .Include(x => x.Unit)
                .ThenInclude(x => x.Faction)
                .SingleOrDefaultAsync(x => x.Id == studyPlanId);
        }

        private Task<DbUniversityMembership> GetMembershipAsync(long userId, DbStudyPlan plan) {
            return db.UniversityMemberships
                .Include(x => x.Player)
                .SingleOrDefaultAsync(x => x.UniversityId == plan.UniversityId && x.Player.UserId == userId);
        }

        private static int LevelToDays(int level) {
            if (level == 1) return 30;

            return LevelToDays(level - 1) + 30 * level;
        }

        public async Task<DbStudyPlan> Handle(SetStudyPlanTarget request, CancellationToken cancellationToken) {
            var plan = await GetPlan(request.StudyPlanId);
            var membership = await GetMembershipAsync(request.UserId, plan);
            if (membership.Role == UniveristyMemberRole.Member) return null;

            plan.Target = new DbSkill {
                Code = request.Skill,
                Level = request.Level,
                Days = LevelToDays(request.Level)
            };

            await db.SaveChangesAsync();

            return plan;
        }

        public async Task<DbStudyPlan> Handle(SetStudPlanyStudy request, CancellationToken cancellationToken) {
            var plan = await GetPlan(request.StudyPlanId);
            var membership = await GetMembershipAsync(request.UserId, plan);
            if (membership.Role == UniveristyMemberRole.Member) return null;

            plan.Teach = new ();
            plan.Study = request.Skill;

            await db.SaveChangesAsync();

            return plan;
        }

        public async Task<DbStudyPlan> Handle(SetStudyPlanTeach request, CancellationToken cancellationToken) {
            var plan = await GetPlan(request.StudyPlanId);
            var membership = await GetMembershipAsync(request.UserId, plan);
            if (membership.Role == UniveristyMemberRole.Member) return null;

            plan.Study = null;
            plan.Teach = request.Units.ToList();

            await db.SaveChangesAsync();

            return plan;
        }
    }
}
