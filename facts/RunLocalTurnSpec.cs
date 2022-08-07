namespace advisor.TurnProcessing;

public class TurnRunnerSpec : IDisposable {
    public TurnRunnerSpec() {
        options = TurnRunnerOptions.UseTempDirectory();
        runner = new TurnRunner(options);
    }

    readonly TurnRunnerOptions options;
    readonly TurnRunner runner;

    public void Dispose() {
        runner.Clean();
        Directory.Delete(options.WorkingDirectory);
    }

    private Task WriteEngineAsync() => runner.WriteEngineAsync(File.ReadAllBytes("data/engine"));
    private Task WriteGameInAsync() => runner.WriteGameAsync(File.ReadAllBytes("data/game.in"));
    private Task WritePlayersInAsync() => runner.WritePlayersAsync(File.ReadAllBytes("data/players.in"));

    private async Task PrepareForTurnAsync() {
        await WriteEngineAsync();
        await WriteGameInAsync();
        await WritePlayersInAsync();
    }

    [Fact]
    public async Task EngineIsWrittenToWorkFolder() {
        await WriteEngineAsync();

        File.Exists(Path.Combine(options.WorkingDirectory, options.EngineFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task GameInIsWrittenToWorkFolder() {
        await WriteGameInAsync();

        File.Exists(Path.Combine(options.WorkingDirectory, options.GameInFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task PlayersInIsWrittenToWorkFolder() {
        await WritePlayersInAsync();

        File.Exists(Path.Combine(options.WorkingDirectory, options.PlayersInFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task CanRunAsync() {
        await PrepareForTurnAsync();

        var result = await runner.RunAsync(TimeSpan.FromMinutes(1));

        result.ExitCode.Should().Be(0);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GameOutFileIsCreatedAfterRun() {
        await PrepareForTurnAsync();

        var result = await runner.RunAsync(TimeSpan.FromMinutes(1));

        File.Exists(Path.Combine(options.WorkingDirectory, options.GameOutFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task PlayersOutFileIsCreatedAfterRun() {
        await PrepareForTurnAsync();

        var result = await runner.RunAsync(TimeSpan.FromMinutes(1));

        File.Exists(Path.Combine(options.WorkingDirectory, options.PlayersOutFileName)).Should().BeTrue();
    }

    [Fact]
    public async Task ReportFilesAreCreatedAfterRun() {
        await PrepareForTurnAsync();

        var result = await runner.RunAsync(TimeSpan.FromMinutes(1));

        var reports = runner.ListReports().ToList();

        reports.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TemplateFilesAreCreatedAfterRun() {
        await PrepareForTurnAsync();

        var result = await runner.RunAsync(TimeSpan.FromMinutes(1));

        var templates = runner.ListTemplates().ToList();

        templates.Count.Should().BeGreaterThan(0);
    }
}
