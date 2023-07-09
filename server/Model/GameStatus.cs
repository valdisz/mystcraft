namespace advisor.Model;

public enum GameStatus {
    /// <summary>
    /// Game just created and not running.
    /// </summary>
    NEW,

    /// <summary>
    /// Game running. Orders can be accepted, registration open.
    /// </summary>
    RUNNING,

    /// <summary>
    /// Game is running. But no orders or new players are accepted.
    /// </summary>
    LOCKED,

    /// <summary>
    /// Game is not running. Orders can be accepted, registration open.
    /// </summary>
    PAUSED,

    /// <summary>
    /// Game is not running, not order or new players are accepted.
    /// </summary>
    STOPED
}
