namespace advisor.Model;

/// <summary>
/// Represents an game engine ID.
/// </summary>
public sealed class EngineId : NewType<EngineId, long> {
    public EngineId(long value) : base(value) { }

    public static new Validation<Error, EngineId> New(long value) =>
        value > 0
            ? Success<Error, EngineId>(new EngineId(value))
            : Fail<Error, EngineId>(Error.New("Engine ID must be greater than 0."));
}
