namespace advisor.Features {
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record StudyPlanTarget(long UserId, string UnitId, string Skill, int Level) : IRequest<StudyPlanResult> {
    }

    public record StudPlanyStudy(long UserId, string UnitId, string Skill) : IRequest<StudyPlanResult> {

    }

    public record StudyPlanTeach(long UserId, string UnitId, int[] Units) : IRequest<StudyPlanResult> {

    }

    public record StudyPlanResult(DbStudyPlan StudyPlan, bool IsSuccess, string Error) : IMutationResult;

    public class SetStudyPlanTargetHandler :
        IRequestHandler<StudyPlanTarget, StudyPlanResult>,
        IRequestHandler<StudPlanyStudy, StudyPlanResult>,
        IRequestHandler<StudyPlanTeach, StudyPlanResult> {
        public SetStudyPlanTargetHandler(Database db) {
            this.db = db;
        }

        private readonly Database db;

        private Task<DbStudyPlan> GetPlan(UnitId id) {

            return db.StudyPlans
                .SingleOrDefaultAsync(x => x.UnitNumber == id.UnitNumber && x.TurnNumber == id.TurnNumber && x.PlayerId == id.PlayerId);
        }

        // private Task<DbAllianceMember> GetMembershipAsync(long userId, DbStudyPlan plan) {
        //     return db.UniversityMemberships
        //         .Include(x => x.Player)
        //         .SingleOrDefaultAsync(x => x.UniversityId == plan.UniversityId && x.Player.UserId == userId);
        // }

        private static int LevelToDays(int level) {
            if (level == 1) return 30;

            return LevelToDays(level - 1) + 30 * level;
        }

        public async Task<StudyPlanResult> Handle(StudyPlanTarget request, CancellationToken cancellationToken) {
            var id = DbUnit.ParseId(request.UnitId);
            var plan = await GetPlan(id);

            if (plan == null) {
                plan = new DbStudyPlan {
                    PlayerId = id.PlayerId,
                    TurnNumber = id.TurnNumber,
                    UnitNumber = id.UnitNumber
                };
                await db.AddAsync(plan);
            }

            plan.Target = new DbSkill {
                Code = request.Skill,
                Level = request.Level,
                Days = LevelToDays(request.Level)
            };

            await db.SaveChangesAsync();

            return new StudyPlanResult(plan, true, null);
        }

        public async Task<StudyPlanResult> Handle(StudPlanyStudy request, CancellationToken cancellationToken) {
            var id = DbUnit.ParseId(request.UnitId);
            var plan = await GetPlan(id);

            if (plan == null) {
                plan = new DbStudyPlan {
                    PlayerId = id.PlayerId,
                    TurnNumber = id.TurnNumber,
                    UnitNumber = id.UnitNumber
                };
                await db.AddAsync(plan);
            }

            plan.Teach = new ();
            plan.Study = request.Skill;

            await db.SaveChangesAsync();

            return new StudyPlanResult(plan, true, null);
        }

        public async Task<StudyPlanResult> Handle(StudyPlanTeach request, CancellationToken cancellationToken) {
            var id = DbUnit.ParseId(request.UnitId);
            var plan = await GetPlan(id);

            if (plan == null) {
                plan = new DbStudyPlan {
                    PlayerId = id.PlayerId,
                    TurnNumber = id.TurnNumber,
                    UnitNumber = id.UnitNumber
                };
                await db.AddAsync(plan);
            }

            plan.Study = null;
            plan.Teach = request.Units.ToList();

            await db.SaveChangesAsync();

            return new StudyPlanResult(plan, true, null);
        }
    }
}
