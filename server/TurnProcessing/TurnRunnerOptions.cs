namespace advisor.TurnProcessing;

using System.IO;
using System.Text.RegularExpressions;

public class TurnRunnerOptions {
    public TurnRunnerOptions(string workingDirectory) {
        WorkingDirectory = workingDirectory;
    }

    public string WorkingDirectory { get; set; }
    public string EngineFileName { get; set; } = "engine";
    public string PlayersInFileName { get; set; } = "players.in";
    public string PlayersOutFileName { get; set; } = "players.out";
    public string GameInFileName { get; set; } = "game.in";
    public string GameOutFileName { get; set; } = "game.out";
    public Regex ReportFileFomat { get; set; } = new Regex(@"report\.(\d+)$", RegexOptions.IgnoreCase);
    public Regex TemplateFileFomat { get; set; } = new Regex(@"template\.(\d+)$", RegexOptions.IgnoreCase);
    public Regex ArticleFileFormat { get; set; } = new Regex(@"times\.(\d+)$", RegexOptions.IgnoreCase);

    public static TurnRunnerOptions UseTempDirectory() {
        var tempPath = Path.GetTempPath();

        string workDir;
        do {
            workDir = Path.Join(tempPath, Path.GetRandomFileName());
        } while (Directory.Exists(workDir));

        Directory.CreateDirectory(workDir);

        return new TurnRunnerOptions(workDir);
    }
}
