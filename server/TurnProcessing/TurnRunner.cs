namespace advisor.TurnProcessing;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using advisor.IO;
using advisor.IO.Traits;
using advisor.Model;
using LanguageExt.Effects.Traits;
using LanguageExt.Sys.Traits;
using LanguageExt.UnsafeValueAccess;

public class TurnRunner<RT> where RT: struct, HasCancel<RT>, HasUnix<RT>, HasDirectory<RT>, HasFile<RT> {
    public TurnRunner(TurnRunnerOptions options) {
        this.options = options;
    }

    public static TurnRunner<RT> New(TurnRunnerOptions options) =>
        new (options);

    public static Aff<RT, A> Use<A>(TurnRunnerOptions options, Func<TurnRunner<RT>, Aff<RT, A>> map) =>
        from runner in Eff(() => New(options))
        from ret in run(runner, map) | @catch(cleanup<A>(runner))
        select ret;

    static Aff<RT, A> run<A>(TurnRunner<RT> runner, Func<TurnRunner<RT>, Aff<RT, A>> map) =>
        from a in map(runner)
        from _ in runner.Clean()
        select a;

    static Aff<RT, A> cleanup<A>(TurnRunner<RT> runner) =>
        from _ in runner.Clean()
        from ret in FailAff<A>(E_OTHER_TURN_PROCESSING_ERROR)
        select ret;

    readonly TurnRunnerOptions options;

    Eff<RT, Seq<FileInfo>> enumerateFiles(Regex pattern) =>
        from items in LanguageExt.Sys.IO.Directory<RT>.enumerateFiles(options.WorkingDirectory)
        select items
            .Where(pattern.IsMatch)
            .Select(item => new FileInfo(item));

    Eff<string> makePath(string name) =>
        Eff(() => Path.Join(options.WorkingDirectory, name));

    public Aff<RT, Unit> WritePlayers(string text) =>
        from path in makePath(options.PlayersInFileName)
        from _ in LanguageExt.Sys.IO.File<RT>.writeAllText(path, text)
        select unit;

    public Aff<RT, Unit> WritePlayers(byte[] bytes) =>
        from path in makePath(options.PlayersInFileName)
        from _ in LanguageExt.Sys.IO.File<RT>.writeAllBytes(path, bytes)
        select unit;

    public Aff<RT, Unit> WriteGame(byte[] bytes) =>
        from path in makePath(options.GameInFileName)
        from _ in LanguageExt.Sys.IO.File<RT>.writeAllBytes(path, bytes)
        select unit;

    public Aff<RT, Unit> WriteFactionOrders(FactionOrders orders) =>
        from path in makePath(options.FactionOrdersFileName(orders.Number))
        let text = orders.Units
            .Map(u => $"unit {u.Number.Value}\n{u.Orders}\n")
            .Aggregate(
                new StringBuilder(),
                (sb, value) => sb.AppendLine(value),
                sb => sb.ToString()
            )
        from _ in LanguageExt.Sys.IO.File<RT>.writeAllText(path, text)
        select unit;

    public Aff<RT, Unit> WriteEngine(byte[] bytes) =>
        from path in makePath(options.EngineFileName)
        from _1 in LanguageExt.Sys.IO.File<RT>.writeAllBytes(path, bytes)
        from _2 in Unix<RT>.Chmod(
            path,
            FilePermission.UserExecute  | FilePermission.UserRead  | FilePermission.UserWrite |
            FilePermission.GroupExecute | FilePermission.GroupRead |
            FilePermission.OtherExecute | FilePermission.OtherRead
        )
        select unit;

    public Aff<RT, Unit> Run(TimeSpan timeout) =>
        from path in makePath(options.EngineFileName)
        from result in AffMaybe<RT, string>(async rt => await RunAsync(path, timeout, rt.CancellationToken))
        select unit;

    async ValueTask<Fin<string>> RunAsync(string engineFileName, TimeSpan timeout, CancellationToken cancellationToken = default) {
        using var p = new Process();
        p.StartInfo = new ProcessStartInfo {
            WorkingDirectory = options.WorkingDirectory,
            FileName = engineFileName,
            Arguments = "run",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        int exitCode = p.ExitCode;
        try {
            p.Start();
            await p.WaitForExitAsync(cts.Token);

            exitCode = p.ExitCode;
        }
        catch (TaskCanceledException) {
            p.Kill(true);
            exitCode = -1;
        }

        var stdout = await p.StandardOutput.ReadToEndAsync();
        if (exitCode != 0) {
            var stderr = await p.StandardError.ReadToEndAsync();
            return Fin<string>.Fail(E_GAME_ENGINE_FAILED(exitCode, stdout, stderr));
        }

        return Fin<string>.Succ(stdout);
    }

    public Eff<FileInfo> GetPlayersOut() =>
        from path in makePath(options.PlayersOutFileName)
        select new FileInfo(path);

    public Eff<FileInfo> GetGameOut() =>
        from path in makePath(options.GameOutFileName)
        select new FileInfo(path);

    Option<FactionNumber> parseFactionNumber(Regex pattern, string fileName) {
        var match = pattern.Match(fileName);
        if (!match.Success) {
            return None;
        }

        if (!int.TryParse(match.Groups[1].Value, out var number)) {
            return None;
        }

        return Some(FactionNumber.New(number));
    }

    public Eff<RT, Seq<FactionFile>> listGameFiles(Regex pattern) =>
        from items in enumerateFiles(pattern)
        select items
            .Select(fileInfo => {
                return parseFactionNumber(pattern, fileInfo.Name)
                    .Match(
                        Some: number => Some(new FactionFile(number, fileInfo)),
                        None: () => None
                    );
            })
            .Where(item => item.IsSome)
            .Select(item => item.Value());

    public Eff<RT, Seq<FactionFile>> ListReports() =>
        listGameFiles(options.ReportFileFomat);

    public Eff<RT, Seq<FactionFile>> ListTemplates() =>
        listGameFiles(options.TemplateFileFomat);

    public Eff<RT, Seq<FactionFile>> ListArticles() =>
        listGameFiles(options.ArticleFileFormat);

    public Eff<RT, Unit> Clean() =>
        LanguageExt.Sys.IO.Directory<RT>.delete(options.WorkingDirectory, true);
}
