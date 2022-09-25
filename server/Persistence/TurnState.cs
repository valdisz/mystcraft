namespace advisor.Persistence;

public enum TurnState {
    /// <summary>
    /// Future turn state.
    /// </summary>
    PENDING,

    /// <summary>
    /// Fully ready - player state updated, stats calculated, etc.
    /// </summary>
    READY
}
