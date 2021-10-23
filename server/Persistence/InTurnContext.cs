namespace advisor.Persistence
{
    public interface InTurnContext : InPlayerContext {
        int TurnNumber { get; set; }
    }
}
