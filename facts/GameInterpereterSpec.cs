namespace advisor.facts;

using advisor;

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
}
