namespace advisor.Hanfgire;

using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using Newtonsoft.Json;

public class JoiningState : IState {
    public JoiningState(
        IEnumerable<string> parentIds,
        IState nextState,
        JobContinuationOptions options,
        TimeSpan expiration) : this(parentIds.ToArray(), new string[0], SerializationHelper.Serialize(nextState, SerializationOption.TypedInternal), options, expiration) {
    }

    public JoiningState(
        IEnumerable<string>  parentIds,
        IEnumerable<string>  completedIds,
        IState nextState,
        JobContinuationOptions options,
        TimeSpan expiration) : this(parentIds.ToArray(), completedIds.ToArray(), SerializationHelper.Serialize(nextState, SerializationOption.TypedInternal), options, expiration) {
    }

    public JoiningState(
        string[] parentIds,
        string[] completedIds,
        string nextState,
        JobContinuationOptions options,
        TimeSpan expiration) {

        ParentIds = parentIds;
        CompletedIds = completedIds;
        NextState = nextState;
        Options = options;
        Expiration = expiration;
    }

    public static readonly string StateName = "Awaiting others";

    public string[] ParentIds { get; }
    public string[] CompletedIds { get; }
    public string NextState { get; }

    public JobContinuationOptions Options { get; }

    [JsonIgnore]
    public TimeSpan Expiration { get; }

    public string Name => StateName;

    public string Reason { get; set; }

    public bool IsFinal => false;

    public bool IgnoreJobLoadException => false;

    public Dictionary<string, string> SerializeData() {
        return new Dictionary<string, string>{
            { "ParentIds", JsonConvert.SerializeObject(ParentIds) },
            { "CompletedIds", JsonConvert.SerializeObject(CompletedIds) },
            { "NextState", NextState },
            { "Options", Options.ToString("G") },
            { "Expiration", Expiration.ToString() }
        };
    }

    internal class Handler : IStateHandler {
        public void Apply(ApplyStateContext context, IWriteOnlyTransaction transaction) {
            transaction.AddToSet("joining", context.BackgroundJob.Id, JobHelper.ToTimestamp(DateTime.UtcNow));
        }

        public void Unapply(ApplyStateContext context, IWriteOnlyTransaction transaction) {
            transaction.RemoveFromSet("joining", context.BackgroundJob.Id);
        }

        public string StateName => JoiningState.StateName;
    }

    public static string[] GetCompletedIds(IDictionary<string, string> data) {
        if (data.TryGetValue("CompletedIds", out var value)) {
            return JsonConvert.DeserializeObject<string[]>(value);
        }

        return new string[0];
    }

    public static void SetCompletedIds(IEnumerable<string> completedIds, IDictionary<string, string> data) {
        data["CompletedIds"] = JsonConvert.SerializeObject(completedIds);
    }

    public static string[] GetParentIds(IDictionary<string, string> data) {
        if (data.TryGetValue("ParentIds", out var value)) {
            return JsonConvert.DeserializeObject<string[]>(value);
        }

        return new string[0];
    }
}
