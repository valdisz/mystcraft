namespace advisor.Persistence;

public enum TurnState {
    /// <summary>
    /// Future turn state.
    /// </summary>
    PENDING,

    /// <summary>
    /// Engine successfully generated new game state.
    /// </summary>
    EXECUTED,

    /// <summary>
    /// All reports have been parsed.
    /// </summary>
    PARSED,

    /// <summary>
    /// All reports have been merged with previos player turn state and alliance turn state.
    /// </summary>
    MERGED,

    /// <summary>
    /// Turn statistics and other actions are ready.
    /// </summary>
    PROCESSED,

    /// <summary>
    /// Fully ready - player state updated, stats calculated, etc.
    /// </summary>
    READY
}
