using advisor.facts.Fixtures;
using advisor.Persistence;

namespace advisor.facts;

public class GameRepositorySpec : WithDatabaseSpec {
    [Fact]
    public async Task newly_created_remote_game_is_in_status_new() {
        var time = new FixedTime(DateTimeOffset.UtcNow);

        using var uow = new UnitOfWork(Db);
        var repo = new GameRepository(time, uow);

        var opt = new GameOptions {

        };

        await uow.BeginTransactionAsync();
        var game = await repo.CreateRemoteAsync("test", "address", "ruleset", opt);
        await uow.CommitTransactionAsync();

        game.CreatedAt.Should().Be(time.UtcNow);
        game.Status.Should().Be(GameStatus.NEW);
    }

    [Fact]
    public async Task newly_created_local_game_is_in_status_new() {
        var time = new FixedTime(DateTimeOffset.UtcNow);

        using var uow = new UnitOfWork(Db);
        var repo = new GameRepository(time, uow);

        var opt = new GameOptions {

        };

        await uow.BeginTransactionAsync();
        var game = await repo.CreateLocalAsync("test", 1, "ruleset", opt);
        await uow.CommitTransactionAsync();

        game.CreatedAt.Should().Be(time.UtcNow);
        game.Status.Should().Be(GameStatus.NEW);
    }
}
