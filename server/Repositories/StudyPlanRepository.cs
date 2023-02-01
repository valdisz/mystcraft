namespace advisor;

using System;
using System.Linq;
using advisor.Persistence;

public interface IStudyPlanRepository
{
    IQueryable<DbStudyPlan> StudyPlans { get; }
}

public class StudyPlanRepository : IStudyPlanRepository
{
    public IQueryable<DbStudyPlan> StudyPlans => throw new NotImplementedException();
}
