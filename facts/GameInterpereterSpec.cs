namespace advisor.facts;

using advisor;
using advisor.facts.Fixtures;
using advisor.Model;
using LanguageExt.Pipes;
using static LanguageExt.Prelude;

public class GameInterpreterSpec {
    [Fact]
    public void CanInterpret() {
        var value = GameId.New(1).Match(
            Succ: gameId => {
                return GameInterpreter<Runtime>.Interpret(Mystcraft.ReadOneGame(gameId));
            },
            Fail: _ => throw new Exception("Should not fail")
        );

        value.Should().NotBeNull();
    }

    [Fact]
    public void CanWriteOneGameEngine() {
        var value = GameEngineId.New(1)
            .Match(
                Succ: gameEngineId =>
                    GameInterpreter<Runtime>.Interpret(
                        from engine in Mystcraft.WriteOneGameEngine(gameEngineId)
                        select engine
                    ),
                Fail: _ => throw new Exception("Should not fail")
            );

        value.Should().NotBeNull();
    }

    [Fact]
    public void CanDeleteGameEngine() {
        var value = GameInterpreter<Runtime>.Interpret(
            from _ in Mystcraft.DeleteGameEngine(new Persistence.DbGameEngine { Id = 1 })
            select _
        );

        value.Should().NotBeNull();
    }
}

public class GameInterpreterIntegrationSpec: WithDatabaseSpec {
    [Fact]
    public async Task ShouldFailIfNoGameEnginePresent() {
        var result = await GameInterpreter<Runtime>.Interpret(
            from engine in Mystcraft.WriteOneGameEngine(new GameEngineId(1))
            select engine
        )
        .Run(Runtime.New(Db));

        result.IfSucc(_ => throw new Exception("Should not succeed"));
        result.IfFail(e => e.Should().Be(Errors.E_GAME_ENGINE_DOES_NOT_EXIST));
    }
}

public class PipesTest {
    [Fact]
    public async Task BoundVariable() {
        var ret = (producer(10) | consumer()).RunEffect().Run(Runtime.New(null));
        var value = await ret.Unwrap();

        // ast bpund variable is returned
        value.Should().Be(-1);
    }

    private static Producer<Runtime, int, int> producer(int prop) =>
        from v in Proxy.lift<Runtime, int>(SuccessEff(0))
        from _ in Proxy.yield(v)
        select prop;

    private static Consumer<Runtime, int, int> consumer() =>
        from v in Proxy.awaiting<int>()
        select -1;
}
