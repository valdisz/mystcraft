namespace advisor.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    public record RunResult(bool Success, int ExitCode, string StdOut, string StdErr);

    public class TurnExecutorOptions {
        public TurnExecutorOptions(string workingDirectory) {
            WorkingDirectory = workingDirectory;
        }

        public string WorkingDirectory { get; set; }
        public string EngineFileName { get; set; } = "engine";
        public Regex ReportFileFomat { get; set; } = new Regex(@"^report\.(\d+)", RegexOptions.IgnoreCase);
        public Regex TimesFileFormat { get; set; } = new Regex(@"^times\.(\d+)", RegexOptions.IgnoreCase);

        public static TurnExecutorOptions UseTempDirectory() {
            string workDir;
            do {
                workDir = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(workDir));

            Directory.CreateDirectory(workDir);

            return new TurnExecutorOptions(workDir);
        }
    }

    public class TurnExecutor {
        public TurnExecutor(TurnExecutorOptions options) {
            this.options = options;
        }

        readonly TurnExecutorOptions options;

        public string MakePath(string name) => Path.Join(options.WorkingDirectory, name);

        public Task WritePlayersInAsync(string s) => File.WriteAllTextAsync(MakePath("players.in"), s);

        public Task WriteGameInAsync(byte[] bytes) => File.WriteAllBytesAsync(MakePath("game.in"), bytes);

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
                FileName = options.EngineFileName,
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

        public Task<byte[]> ReadPlayersOutAsync() => File.ReadAllBytesAsync(MakePath("players.out"));

        public Stream OpenPlayersOut() => File.OpenRead(MakePath("players.out"));

        public Task<byte[]> ReadGameOutAsync() => File.ReadAllBytesAsync(MakePath("game.out"));

        public IEnumerable<FactionReport> ListReports() {
            foreach (var fn in Directory.EnumerateFiles(options.WorkingDirectory)) {
                var m = options.ReportFileFomat.Match(fn);
                if (m.Success) {
                    var number = int.Parse(m.Captures[1].Value);
                    yield return new FactionReport(number, File.OpenRead(fn));
                }
            }
        }

        public IEnumerable<Stream> ListTimes() {
            foreach (var fn in Directory.EnumerateFiles(options.WorkingDirectory)) {
                if (options.TimesFileFormat.IsMatch(fn)) {
                    // var number = int.Parse(fn.Substring(TIMES.Length));
                    yield return File.OpenRead(fn);
                }
            }
        }
    }
}
