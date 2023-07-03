namespace advisor;

/// <summary>
/// Represents a player ID.
/// </summary>
public sealed class PlayerId : NewType<PlayerId, long> {
    public PlayerId(long value) : base(value) { }
}
