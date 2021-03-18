namespace advisor
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using HotChocolate;
    using HotChocolate.Types;
    using HotChocolate.Types.Relay;
    using Microsoft.EntityFrameworkCore;
    using Persistence;

    public class UniversityClass {
        public UniversityClass(long universityId, int turnNumber) {
            this.universityId = universityId;
            TurnNumber = turnNumber;
        }

        public UniversityClass(string id) {
            var parsedId = id.Split(":");

            universityId = long.Parse(parsedId[0]);
            TurnNumber = int.Parse(parsedId[1]);
        }

        private readonly long universityId;

        public string Id => $"{universityId}:{TurnNumber}";

        public int TurnNumber { get; }

        public async Task<List<DbStudyPlan>> Students([Service] Database db) {
            var students = await db.StudyPlans
                .Include(x => x.Turn)
                .Include(x => x.Unit)
                .ThenInclude(x => x.Faction)
                .Where(x => x.UniversityId == universityId && x.Turn.Number == TurnNumber)
                .ToListAsync();

            return students;
        }
    }

    public class UniversityClassType : ObjectType<UniversityClass> {
        protected override void Configure(IObjectTypeDescriptor<UniversityClass> descriptor) {
            descriptor.AsNode()
                .IdField(x => x.Id)
                .NodeResolver((ctx, id) => {
                    return Task.FromResult(new UniversityClass(id));
                });
        }
    }
}
