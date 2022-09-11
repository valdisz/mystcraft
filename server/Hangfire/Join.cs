namespace advisor.Hanfgire;

using Hangfire;

public record Join(string JobId, JobContinuationOptions Options);
