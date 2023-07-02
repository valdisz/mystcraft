namespace advisor.Persistence;

/// <summary>
/// This interface is used to mark entities that are in the context of a game.
/// </summary>
public interface InGameContext
{
    long GameId { get; }
}
