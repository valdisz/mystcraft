namespace advisor.Features;

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
        var turnNumbers = await db.PlayerTurns
            .OnlyPlayer(request.PlayerId)
            .OrderByDescending(x => x.Number)
            .Select(x => x.Number)
            .Take(2)
            .ToListAsync();

        if (turnNumbers.Count != 2) {
            return Unit.Value;
        }

        var turnNumber = turnNumbers[0];
        var prevTurnNumber = turnNumbers[1];

        var prevPlans = (await db.StudyPlans
            .AsNoTracking()
            .InTurn(request.PlayerId, prevTurnNumber)
            .ToListAsync())
            .ToDictionary(x => x.UnitNumber);

        var currentPlans = (await db.StudyPlans
            .AsNoTracking()
            .InTurn(request.PlayerId, turnNumber)
            .ToListAsync())
            .ToDictionary(x => x.UnitNumber);

        foreach (var ( unitNumber, oldPlan ) in prevPlans) {
            if (currentPlans.ContainsKey(unitNumber)) {
                continue;
            }

            var unitExist = await db.Units
                .AsNoTracking()
                .InTurn(request.PlayerId, turnNumber)
                .AnyAsync(x => x.Number == unitNumber);

            if (unitExist) {
                var plan = new DbStudyPlan {
                    PlayerId = request.PlayerId,
                    TurnNumber = turnNumber,
                    UnitNumber = unitNumber,
                    Target = oldPlan.Target
                };

                await db.AddAsync(plan);
            }
        }

        await db.SaveChangesAsync();

        return Unit.Value;
    }
}
