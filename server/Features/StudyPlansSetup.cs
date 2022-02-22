namespace advisor.Features {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Persistence;
    using AutoMapper;
    using MediatR;
    using Microsoft.EntityFrameworkCore;

    public record StudyPlansSetup(long PlayerId) : IRequest {

    }

    public class StudyPlansSetupHandler : IRequestHandler<StudyPlansSetup> {
        public StudyPlansSetupHandler(Database db, IMapper mapper) {
            this.db = db;
            this.mapper = mapper;
        }

        private readonly Database db;
        private readonly IMapper mapper;

        public async Task<Unit> Handle(StudyPlansSetup request, CancellationToken cancellationToken) {
            var turnNumbers = await db.Turns
                .FilterByPlayer(request.PlayerId)
                .OrderByDescending(x => x.Number)
                .Select(x => x.Number)
                .Take(2)
                .ToListAsync();

            if (turnNumbers.Count != 2) {
                return Unit.Value;
            }

            var lastTurn = turnNumbers[0];
            var prevTurn = turnNumbers[1];

            var prevPlans = (await db.StudyPlans
                .FilterByTurn(request.PlayerId, prevTurn)
                .ToListAsync())
                .ToDictionary(x => x.UnitNumber);

            var currentPlans = (await db.StudyPlans
                .FilterByTurn(request.PlayerId, lastTurn)
                .ToListAsync())
                .ToDictionary(x => x.UnitNumber);

            foreach (var kv in prevPlans) {
                if (currentPlans.ContainsKey(kv.Key)) {
                    continue;
                }

                var unitExist = await db.Units
                    .AsNoTracking()
                    .FilterByTurn(request.PlayerId, lastTurn)
                    .AnyAsync(x => x.Number == kv.Key);

                if (unitExist) {
                    var plan = mapper.Map<DbStudyPlan>(kv.Value);
                    plan.TurnNumber = lastTurn;

                    await db.AddAsync(plan);
                }
            }

            await db.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
