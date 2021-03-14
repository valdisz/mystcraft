namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public record UniversityClass(long UniversityId, int TurnNumber) {
        public Task<List<DbStudyPlan>> Students([Service] Database db) {
            return db.StudyPlans
                .Include(x => x.Turn)
                .Include(x => x.Unit)
                .ThenInclude(x => x.Faction)
                .Where(x => x.UniversityId == UniversityId && x.Turn.Number == TurnNumber)
                .ToListAsync();
        }
    }
}
