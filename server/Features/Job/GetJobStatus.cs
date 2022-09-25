namespace advisor.Features;

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Hangfire;
using AutoMapper;
using Hangfire.States;

public record GetJobStatus(string JobId): IRequest<BackgroundJob>;

public record BackgroundJob(string Id, JobState State, string Reason);

public class GetJobStatusHandler : IRequestHandler<GetJobStatus, BackgroundJob> {
    public GetJobStatusHandler(JobStorage jobStorage, IMapper mapper) {
        this.jobStorage = jobStorage;
        this.mapper = mapper;
    }

    private readonly JobStorage jobStorage;
    private readonly IMapper mapper;

    public Task<BackgroundJob> Handle(GetJobStatus request, CancellationToken cancellationToken) {
        using var conn = jobStorage.GetConnection();
        var state = conn.GetStateData(request.JobId);

        var result = new BackgroundJob(request.JobId, MapState(state.Name), state.Reason);

        return Task.FromResult(result);
    }

    private static JobState MapState(string state) {
        if (state == AwaitingState.StateName) return JobState.PENDING;
        if (state == EnqueuedState.StateName) return JobState.PENDING;
        if (state == ScheduledState.StateName) return JobState.PENDING;
        if (state == ProcessingState.StateName) return JobState.RUNNING;
        if (state == SucceededState.StateName) return JobState.SUCCEEDED;
        if (state == FailedState.StateName) return JobState.FAILED;
        if (state == DeletedState.StateName) return JobState.DELETED;

        return JobState.UNKNOWN;
    }
}
