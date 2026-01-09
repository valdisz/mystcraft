namespace advisor.TurnProcessing;

using System;
using System.IO;
using System.Text.RegularExpressions;
using advisor.Model;

public record TurnRunnerOptions {
    public required string WorkingDirectory { get; init; }
    public string EngineFileName { get; init; } = "engine";
    public string PlayersInFileName { get; init; } = "players.in";
    public string PlayersOutFileName { get; init; } = "players.out";
    public string GameInFileName { get; init; } = "game.in";
    public string GameOutFileName { get; init; } = "game.out";
    public Regex ReportFileFomat { get; init; } = new Regex(@"report\.(\d+)$", RegexOptions.IgnoreCase);
    public Regex TemplateFileFomat { get; init; } = new Regex(@"template\.(\d+)$", RegexOptions.IgnoreCase);
    public Regex ArticleFileFormat { get; init; } = new Regex(@"times\.(\d+)$", RegexOptions.IgnoreCase);
    public Func<FactionNumber, string> FactionOrdersFileName { get; init; } = number => $"orders.{number.Value}";

    public static Try<Option<TurnRunnerOptions>> UseTempDirectory(int attempts = 1000) {
        return () =>
        {
            var tempPath = Path.GetTempPath();

            string workDir;
            int iter = 0;
            do
            {
                workDir = Path.Join(tempPath, Path.GetRandomFileName());
                if (++iter > attempts)
                {
                    return Option<TurnRunnerOptions>.None;
                }
            } while (Directory.Exists(workDir));

            Directory.CreateDirectory(workDir);

            return Some(New(workDir));
        };
    }

    public static TurnRunnerOptions New(string workingDirectory)
        => new () { WorkingDirectory = workingDirectory };
}
