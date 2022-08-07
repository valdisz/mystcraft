namespace advisor.TurnProcessing;

public record RunResult(bool Success, int ExitCode, string StdOut, string StdErr);
