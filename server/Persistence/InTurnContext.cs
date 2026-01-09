namespace advisor.Persistence;

/// <summary>
/// Marks an entity that it is connected to a turn.
/// </summary>
public interface InTurnContext : InPlayerContext {
    int TurnNumber { get; set; }
}
