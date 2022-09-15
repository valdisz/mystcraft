namespace advisor.Persistence;

public enum TurnStatus {
    /// <summary>
    /// Future turn state.
    /// </summary>
    PENDING,

    /// <summary>
    /// Next turn is executed, parsed and imported.
    /// </summary>
    EXECUTED,

    /// <summary>
    /// Fully ready - player state updated, stats calculated, etc.
    /// </summary>
    READY,

    /// <summary>
    /// Errors during parsing.
    /// </summary>
    ERROR
}
