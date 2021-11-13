namespace advisor.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class TurnExecutor {
        string workDir;

        public string MakeName(string name) => Path.Join(workDir, "engine");

        public Task WritePlayersInAsync(string s) => File.WriteAllTextAsync(MakeName("players.in"), s);

        public Task WriteGameInAsync(byte[] bytes) => File.WriteAllBytesAsync(MakeName("game.in"), bytes);

        public void CreateWorkDirectory() {
            do {
                workDir = Path.Join(Path.GetTempPath(), Path.GetRandomFileName());
            } while (Directory.Exists(workDir));

            Directory.CreateDirectory(workDir);
        }

        public async Task WriteFactionOrdersAsync(int faction, string password, IEnumerable<UnitOrdersRec> orders) {
            using var fs = File.Open(MakeName($"orders.{faction}"), FileMode.CreateNew, FileAccess.Write);
            using var w = new StreamWriter(fs, Encoding.UTF8);

            await w.WriteLineAsync($"#atlantis {faction} \"{password}\"");

            foreach (var ( number, unitOrders ) in orders) {
                await w.WriteLineAsync($"unit {number}");
                await w.WriteLineAsync(unitOrders);
            }

            await w.WriteLineAsync("#end");
        }

        public async Task<bool> RunAsync(byte[] engine, TimeSpan timeout, CancellationToken cancellationToken = default) {
            var enginePath = MakeName("engine");
            await File.WriteAllBytesAsync(enginePath, engine);

            using var p = new Process();
            p.StartInfo = new ProcessStartInfo {
                WorkingDirectory = workDir,
                FileName = enginePath,
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
            }

            var exitCode = p.ExitCode;
            var success = !cts.IsCancellationRequested && exitCode == 0;

            var output = await p.StandardOutput.ReadToEndAsync();
            var error = await p.StandardError.ReadToEndAsync();

            return success;
        }

        public Task<byte[]> ReadPlayersOutAsync() => File.ReadAllBytesAsync(MakeName("players.out"));

        public Stream OpenPlayersOut() => File.OpenRead(MakeName("players.out"));

        public Task<byte[]> ReadGameOutAsync() => File.ReadAllBytesAsync(MakeName("game.out"));

        const string REPORT = "report.";
        const string TIMES = "times.";

        public IEnumerable<FactionReport> ReadReports() {
            foreach (var fn in Directory.EnumerateFiles(workDir)) {
                if (fn.StartsWith(REPORT, ignoreCase: true, culture: null)) {
                    var number = int.Parse(fn.Substring(REPORT.Length));
                    yield return new FactionReport(number, File.OpenRead(fn));
                }
            }
        }

        public IEnumerable<Stream> ReadTimesAsync() {
            foreach (var fn in Directory.EnumerateFiles(workDir)) {
                if (fn.StartsWith(TIMES, ignoreCase: true, culture: null)) {
                    var number = int.Parse(fn.Substring(TIMES.Length));
                    yield return File.OpenRead(fn);
                }
            }
        }
    }
}
