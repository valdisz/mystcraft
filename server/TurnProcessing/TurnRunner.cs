namespace advisor.TurnProcessing;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class TurnRunner {
    public TurnRunner(TurnRunnerOptions options) {
        this.options = options;
    }

    readonly TurnRunnerOptions options;

    public string MakePath(string name) => Path.Join(options.WorkingDirectory, name);

    public Task WritePlayersAsync(string s) => File.WriteAllTextAsync(MakePath(options.PlayersInFileName), s);

    public Task WritePlayersAsync(byte[] bytes) => File.WriteAllBytesAsync(MakePath(options.PlayersInFileName), bytes);

    public Task WriteGameAsync(byte[] bytes) => File.WriteAllBytesAsync(MakePath(options.GameInFileName), bytes);

    public async Task WriteFactionOrdersAsync(int faction, string password, IEnumerable<UnitOrdersRec> orders) {
        await using var fs = File.Open(MakePath($"orders.{faction}"), FileMode.CreateNew, FileAccess.Write);
        await using var w = new StreamWriter(fs, Encoding.UTF8);

        await w.WriteLineAsync($"#atlantis {faction} \"{password}\"");

        foreach (var ( number, unitOrders ) in orders) {
            await w.WriteLineAsync($"unit {number}");
            await w.WriteLineAsync(unitOrders);
        }

        await w.WriteLineAsync("#end");
    }

    public async Task WriteEngineAsync(byte[] bytes) {
        var enginePath = MakePath(options.EngineFileName);

        await File.WriteAllBytesAsync(enginePath, bytes);
        UnixInterop.chmod(
            enginePath,
            UnixInterop.S_IXUSR | UnixInterop.S_IRUSR | UnixInterop.S_IWUSR |
            UnixInterop.S_IXGRP | UnixInterop.S_IRGRP |
            UnixInterop.S_IXOTH | UnixInterop.S_IROTH
        );
    }

    public async Task<RunResult> RunAsync(TimeSpan timeout, CancellationToken cancellationToken = default) {
        using var p = new Process();
        p.StartInfo = new ProcessStartInfo {
            WorkingDirectory = options.WorkingDirectory,
            FileName = MakePath(options.EngineFileName),
            Arguments = "run",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try {
            p.Start();
            await p.WaitForExitAsync(cts.Token);
        }
        catch (TaskCanceledException) {
            p.Kill(true);

            throw;
        }

        var exitCode = p.ExitCode;
        var success = exitCode == 0;
        var stdout = await p.StandardOutput.ReadToEndAsync();
        var stderr = await p.StandardError.ReadToEndAsync();

        return new RunResult(success, exitCode, stdout, stderr);
    }

    public FileInfo GetPlayersOut() => new FileInfo(MakePath(options.PlayersOutFileName));

    public FileInfo GetGameOut() => new FileInfo(MakePath(options.GameOutFileName));

    public IEnumerable<FactionFile> ListReports() {
        return ListFiles(options.ReportFileFomat)
            .Select(item => {
                var (match, fileInfo) = item;

                var number = int.Parse(match.Groups[1].Value);
                return new FactionFile(number, fileInfo);
            });
    }

    public IEnumerable<FactionFile> ListTemplates() {
        return ListFiles(options.TemplateFileFomat)
            .Select(item => {
                var (match, fileInfo) = item;

                var number = int.Parse(match.Groups[1].Value);
                return new FactionFile(number, fileInfo);
            });
    }

    public IEnumerable<FileInfo> ListArticles()
        => ListFiles(options.ArticleFileFormat)
            .Select(x => x.fileInfo);

    public void Clean() {
        var dir = new DirectoryInfo(options.WorkingDirectory);
        foreach (var file in dir.EnumerateFiles()) {
            file.Delete();
        }
    }

    private IEnumerable<(Match match, FileInfo fileInfo)> ListFiles(Regex pattern) {
        foreach (var fn in Directory.EnumerateFiles(options.WorkingDirectory)) {
            var match = pattern.Match(fn);
            if (match.Success) {
                yield return (match, new FileInfo(fn));
            }
        }
    }
}
