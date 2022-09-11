namespace advisor.Hanfgire;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

public class JoiningSupportAttribute : JobFilterAttribute, IElectStateFilter, IApplyStateFilter {
    public JoiningSupportAttribute(IBackgroundJobStateChanger stateChanger) {
        this.stateChanger = stateChanger;
    }

    private readonly IBackgroundJobStateChanger stateChanger;

    private readonly HashSet<string> knownFinalStates = new HashSet<string> {
        DeletedState.StateName,
        SucceededState.StateName
    };

    public void OnStateElection(ElectStateContext context) {
        var joining = context.CandidateState as JoiningState;
        if (joining != null) {
            // Branch for a child background job.
            AddJoining(context, joining);
        }
        else if (knownFinalStates.Contains(context.CandidateState.Name)) {
            // Branch for a parent background job.
            ExecuteJoinsIfExist(context);
        }
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        var awaitingState = context.NewState as JoiningState;
        if (awaitingState != null)
        {
            context.JobExpirationTimeout = awaitingState.Expiration;
        }
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    private void AddJoining(ElectStateContext context, JoiningState joining) {
        var connection = context.Connection;
        var currentJobId = context.BackgroundJob.Id;

        // We store continuations as a json array in a job parameter. Since there
        // is no way to add a continuation in an atomic way, we are placing a
        // distributed lock on parent job to prevent race conditions, when
        // multiple threads add continuation to the same parent job.

        var jobStates = new Dictionary<string, StateData>();
        foreach (var parentId in joining.ParentIds) {
            using (connection.AcquireDistributedJobLock(parentId, TimeSpan.FromMinutes(1))) {
                var jobData = connection.GetJobData(parentId);
                if (jobData == null) {
                    // When we try to add a continuation for a removed job,
                    // the system should throw an exception instead of creating
                    // corrupted state.
                    throw new InvalidOperationException($"Can not add a continuation: parent background job '{parentId}' does not exist.");
                }

                var joins = GetJoins(connection, parentId);

                // Continuation may be already added. This may happen, when outer transaction
                // was failed after adding a continuation last time, since the addition is
                // performed outside of an outer transaction.
                if (!joins.Exists(x => x.JobId == currentJobId)) {
                    joins.Add(new Join(JobId: context.BackgroundJob.Id, Options: joining.Options));

                    // Set continuation only after ensuring that parent job still
                    // exists. Otherwise we could create add non-expiring (garbage)
                    // parameter for the parent job.
                    SetJoins(connection, parentId, joins);
                }

                var currentState = connection.GetStateData(parentId);
                jobStates.Add(parentId, currentState);
            }
        }

        if (jobStates.Values.All(state => state != null && knownFinalStates.Contains(state.Name))) {
            context.CandidateState = SerializationHelper.Deserialize<IState>(joining.NextState, SerializationOption.TypedInternal);
        }
    }

    private void ExecuteJoinsIfExist(ElectStateContext context) {
        var connection = context.Connection;

        // The following lines are executed inside a distributed job lock,
        // so it is safe to get continuation list here.
        var joins = GetJoins(context.Connection, context.BackgroundJob.Id);
        var nextStates = new Dictionary<string, IState>();

        // Getting continuation data for all continuations – state they are waiting
        // for and their next state.
        foreach (var join in joins)
        {
            if (String.IsNullOrWhiteSpace(join.JobId)) continue;

            using (connection.AcquireDistributedJobLock(join.JobId, TimeSpan.FromMinutes(1))){
                var currentState = GetJoinState(context, join.JobId, TimeSpan.FromMinutes(1));
                if (currentState == null) {
                    continue;
                }

                // All joins should be in the joining state. If someone changed
                // the state of a continuation, we should simply skip it.
                if (currentState.Name != JoiningState.StateName) continue;

                IState nextState;

                if (join.Options.HasFlag(JobContinuationOptions.OnlyOnSucceededState) && context.CandidateState.Name != SucceededState.StateName) {
                    nextState = new DeletedState { Reason = "Continuation condition was not met" };
                }
                else {
                    try {
                        var parents = JoiningState.GetParentIds(currentState.Data);
                        var completedIds = new HashSet<string>(JoiningState.GetCompletedIds(currentState.Data));
                        completedIds.Add(context.BackgroundJob.Id);

                        if (completedIds.Count != parents.Length) {
                            stateChanger.ChangeState(new StateChangeContext(
                                context.Storage,
                                context.Connection,
                                join.JobId,
                                new JoiningState(
                                    parents,
                                    completedIds.ToArray(),
                                    currentState.Data["NextState"],
                                    Enum.Parse<JobContinuationOptions>(currentState.Data["Options"]),
                                    TimeSpan.Parse(currentState.Data["Expiration"])
                                ),
                                JoiningState.StateName
                            ));

                            continue;
                        }

                        nextState = SerializationHelper.Deserialize<IState>(currentState.Data["NextState"], SerializationOption.TypedInternal);
                    }
                    catch (Exception ex) {
                        nextState = new FailedState(ex) {
                            Reason = "An error occurred while deserializing the continuation"
                        };
                    }
                }

                if (!nextStates.ContainsKey(join.JobId))
                {
                    // Duplicate continuations possible, when they were added before version 1.6.10.
                    // Please see details in comments for the AddContinuation method near the line
                    // with checking for existence (continuations.Exists).
                    nextStates.Add(join.JobId, nextState);
                }
            }
        }

        foreach (var tuple in nextStates) {
            stateChanger.ChangeState(new StateChangeContext(
                context.Storage,
                context.Connection,
                tuple.Key,
                tuple.Value,
                JoiningState.StateName
            ));
        }
    }

    private StateData GetJoinState(ElectStateContext context, string joinJobId, TimeSpan timeout) {
        StateData currentState = null;

        var started = Stopwatch.StartNew();
        var firstAttempt = true;

        while (true) {
            var joinData = context.Connection.GetJobData(joinJobId);
            if (joinData == null) {
                // _logger.Warn(
                //     $"Can not start continuation '{continuationJobId}' for background job '{context.BackgroundJob.Id}': continuation does not exist.");

                break;
            }

            currentState = context.Connection.GetStateData(joinJobId);
            if (currentState != null) {
                break;
            }

            if (DateTime.UtcNow - joinData.CreatedAt > TimeSpan.FromMinutes(15)) {
                // _logger.Warn(
                //     $"Continuation '{continuationJobId}' has been ignored: it was deemed to be aborted, because its state is still non-initialized.");

                break;
            }

            if (started.Elapsed >= timeout) {
                // _logger.Warn(
                //     $"Can not start continuation '{continuationJobId}' for background job '{context.BackgroundJob.Id}': timeout expired while trying to fetch continuation state.");

                break;
            }

            Thread.Sleep(firstAttempt ? 0 : 100);
            firstAttempt = false;
        }

        return currentState;
    }

    const string JOIN_PARAMETER = "Join";

    private static void SetJoins(IStorageConnection connection, string jobId, List<Join> continuations)
    {
        connection.SetJobParameter(jobId, JOIN_PARAMETER, SerializationHelper.Serialize(continuations));
    }

    private static List<Join> GetJoins(IStorageConnection connection, string jobId)
    {
        return DeserializeJoins(connection.GetJobParameter(jobId, JOIN_PARAMETER));
    }

    internal static List<Join> DeserializeJoins(string serialized)
    {
        var list = SerializationHelper.Deserialize<List<Join>>(serialized);
        if (list != null && list.TrueForAll(x => x.JobId == null)) {
            list = SerializationHelper.Deserialize<List<Join>>(serialized, SerializationOption.User);
        }

        return list ?? new List<Join>();
    }
}
