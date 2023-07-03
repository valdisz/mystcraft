namespace advisor;

/// <summary>
/// Represents a game ID.
/// </summary>
public sealed class GameId : NewType<GameId, long> {
    public GameId(long value) : base(value) { }
}
