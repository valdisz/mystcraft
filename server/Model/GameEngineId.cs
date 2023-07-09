namespace advisor.Model;

/// <summary>
/// Represents a game ID.
/// </summary>
public sealed class GameEngineId : NewType<GameEngineId, long> {
    public GameEngineId(long value) : base(value) { }

    public static new Validation<Error, GameEngineId> New(long value) =>
        value > 0
            ? Success<Error, GameEngineId>(new GameEngineId(value))
            : Fail<Error, GameEngineId>(Error.New("Game Engine ID must be greater than 0."));

    public static new Validation<Error, GameEngineId> New(HotChocolate.Types.Relay.IdValue value) =>
        value.TypeName == advisor.Schema.GameEngineType.NAME
            ? Success<Error, GameEngineId>(new GameEngineId((long) value.Value))
            : Fail<Error, GameEngineId>(Error.New("Provided value is not Game Engine ID."));
}
