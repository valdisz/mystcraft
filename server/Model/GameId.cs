namespace advisor.Model;

/// <summary>
/// Represents a game ID.
/// </summary>
public sealed class GameId : NewType<GameId, long> {
    public GameId(long value) : base(value) { }

    public static new Validation<Error, GameId> New(long value) =>
        value > 0
            ? Success<Error, GameId>(new GameId(value))
            : Fail<Error, GameId>(Error.New("Game ID must be greater than 0."));

    public static new Validation<Error, GameId> New(HotChocolate.Types.Relay.IdValue value) =>
        value.TypeName == advisor.Schema.GameType.NAME
            ? Success<Error, GameId>(new GameId((long) value.Value))
            : Fail<Error, GameId>(Error.New("Provided value is not Game ID."));
}
