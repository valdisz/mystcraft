// namespace advisor
// {
//     using System;
//     using System.Collections.Generic;
//     using System.Linq;
//     using System.Threading.Tasks;
//     using HotChocolate;
//     using HotChocolate.Types;
//     using HotChocolate.Types.Relay;
//     using Microsoft.EntityFrameworkCore;
//     using Persistence;

//     public class UniversityClass {
//         public UniversityClass(long universityId, int turnNumber) {
//             this.universityId = universityId;
//             TurnNumber = turnNumber;
//         }

//         public UniversityClass(string id) {
//             var parsedId = id.Split(":");

//             universityId = long.Parse(parsedId[0]);
//             TurnNumber = int.Parse(parsedId[1]);
//         }

//         private readonly long universityId;

//         public string Id => $"{universityId}:{TurnNumber}";

//         public int TurnNumber { get; }

//         public async Task<List<DbStudyPlan>> Students([Service] Database db) {
//             var query = db.StudyPlans
//                 .AsNoTracking()
//                 .Include(x => x.Turn)
//                 .Include(x => x.Unit)
//                 .ThenInclude(x => x.Faction)
//                 .Where(x => x.UniversityId == universityId && x.Turn.Number == TurnNumber)
//                 .Select(x => new {
//                     x.Id,
//                     x.Study,
//                     x.Target,
//                     x.Teach,
//                     x.TurnId,
//                     x.UniversityId,
//                     x.UnitId,
//                     UnitNumber = x.Unit.Number,
//                     UnitName = x.Unit.Name,
//                     UnitFactionId = x.Unit.FactionId,
//                     UnitFaction = x.Unit.Faction,
//                     UnitSkills = x.Unit.Skills,
//                     UnitCanStudy = x.Unit.CanStudy,
//                     UnitRegionId = x.Unit.RegionId
//                 });

//             var students = (await query.ToListAsync())
//                 .Select(x => new DbStudyPlan {
//                     Id = x.Id,
//                     Study = x.Study,
//                     Target = x.Target,
//                     Teach = x.Teach,
//                     TurnId = x.TurnId,
//                     UniversityId = x.UniversityId,
//                     UnitId = x.UnitId,
//                     Unit = new DbUnit {
//                         Id = x.UnitId,
//                         Name = x.UnitName,
//                         Number = x.UnitNumber,
//                         FactionId = x.UnitFactionId,
//                         Faction = x.UnitFaction,
//                         Skills = x.UnitSkills,
//                         CanStudy = x.UnitCanStudy,
//                         RegionId = x.UnitRegionId
//                     }
//                 })
//                 .ToList();

//             var regions = new Dictionary<long, DbRegion>();
//             foreach (var s in students) {
//                 var regId = s.Unit.RegionId;
//                 if (!regions.ContainsKey(regId)) {
//                     var reg = await db.Regions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == regId);
//                     regions.Add(regId, reg);
//                 }

//                 s.Unit.Region = regions[regId];
//             }

//             return students;
//         }
//     }

//     public class UniversityClassType : ObjectType<UniversityClass> {
//         protected override void Configure(IObjectTypeDescriptor<UniversityClass> descriptor) {
//             descriptor.AsNode()
//                 .IdField(x => x.Id)
//                 .NodeResolver((ctx, id) => {
//                     return Task.FromResult(new UniversityClass(id));
//                 });
//         }
//     }
// }
