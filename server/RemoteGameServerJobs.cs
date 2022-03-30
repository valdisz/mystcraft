namespace advisor {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using advisor.Features;
    using advisor.Persistence;
    using AngleSharp;
    using Hangfire;
    using Hangfire.Client;
    using Hangfire.Common;
    using Hangfire.Console;
    using Hangfire.RecurringJobExtensions;
    using Hangfire.Server;
    using Hangfire.States;
    using Hangfire.Storage;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    public class RemoteGameServerJobs {
        public RemoteGameServerJobs(Database db, IMediator mediator, IHttpClientFactory httpClientFactory,
            IBackgroundJobClient backgroundJobs,
            ILogger<RemoteGameServerJobs> logger) {
            this.db = db;
            this.mediator = mediator;
            this.httpClientFactory = httpClientFactory;
            this.backgroundJobs = backgroundJobs;
            this.logger = logger;
        }

        private readonly Database db;
        private readonly IMediator mediator;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IBackgroundJobClient backgroundJobs;
        private readonly ILogger logger;

        // [RecurringJob("0 12 * * 2,5", TimeZone = "America/Los_Angeles")]
        public async Task NewOrigins(long gameId) {
            var players = await db.Players
                .AsNoTracking()
                .Where(x => x.GameId == gameId && x.Number != null && x.Password != null && !x.IsQuit)
                .ToListAsync();

            if (players.Count == 0) {
                logger.LogInformation($"No factions with known password, report downloading completed.");
            }

            var missingTurns = new List<DbPlayer>();

            var started = DateTime.UtcNow;
            int remoteTurnNumber;
            do {
                remoteTurnNumber = await GetRemoteTurnNumberAsync();
                logger.LogInformation($"Remote turn number is {remoteTurnNumber}");

                foreach (var player in players) {
                    if (player.LastTurnNumber < remoteTurnNumber) {
                        missingTurns.Add(player);
                    }
                }

                if (missingTurns.Count == 0) {
                    logger.LogInformation($"Sleep 1 minute");
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
            while (missingTurns.Count == 0 && (TimeSpan.FromMinutes(30) > (DateTime.UtcNow - started)));

            if (missingTurns.Count > 0) {
                await DownloadReportsAsync(missingTurns);
                await ProcessTurns(missingTurns, remoteTurnNumber);

                logger.LogInformation($"All player reports were processed");
            }
            else {
                logger.LogError($"New turn was not generated or was not possible to reach server");
            }
        }

        private async Task<int> GetRemoteTurnNumberAsync() {
            logger.LogInformation("Scraping remote game state");

            var config = Configuration.Default.WithDefaultLoader();

            var browsingContext = BrowsingContext.New(config);
            var document = await browsingContext.OpenAsync("http://atlantis-pbem.com/");

            var allHeadings = document.QuerySelectorAll("h3");
            foreach (var h in allHeadings) {
                if (h.TextContent.StartsWith("Turn Number:")) {
                    var turnNumber = int.Parse(h.QuerySelector("span").TextContent);
                    return turnNumber;
                }
            }

            return -1;
        }

        public async Task DownloadReportsAsync(List<DbPlayer> players) {
            foreach (var player in players) {
                logger.LogInformation($"{player.Name} ({player.Number}): Downloading new turn from the server");
                var report = await DownloadReportForFactionAsync(player.Number.Value, player.Password);

                logger.LogInformation($"{player.Name} ({player.Number}): Saving report to database");
                var turn = await mediator.Send(new PlayerReportUpload(player.Id, new[] { report }));
            }
        }

        private async Task<string> DownloadReportForFactionAsync(int factionNumber, string password) {
            using var http = httpClientFactory.CreateClient();

            var fields = new Dictionary<string, string>();
            fields.Add("factionId", factionNumber.ToString());
            fields.Add("password", password);

            var request = new HttpRequestMessage(HttpMethod.Post, "http://atlantis-pbem.com/game/download-report") {
                Content = new FormUrlEncodedContent(fields)
            };

            var response = await http.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var report = await response.Content.ReadAsStringAsync();
            return report;
        }

        public async Task ProcessTurns(List<DbPlayer> players, int turnNumber) {
            foreach (var player in players) {
                try {
                    logger.LogInformation($"{player.Name} ({player.Number}): Processing turn");
                    await mediator.Send(new TurnProcess(player.Id, turnNumber));
                }
                catch (Exception ex) {
                    logger.LogError(ex, $"{player.Name} ({player.Number}): {ex.Message}");
                    throw;
                }
            }
        }
    }

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

    public record Join(string JobId, JobContinuationOptions Options);

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
}
