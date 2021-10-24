namespace advisor.Features
{
    using advisor.Persistence;

    public record TurnContext : InTurnContext {
        public TurnContext(long playerId, int turnNumber) {
            TurnNumber = turnNumber;
            PlayerId = playerId;
        }

        public TurnContext(TurnContext other) {
            TurnNumber = other.TurnNumber;
            PlayerId = other.PlayerId;
        }

        public long PlayerId { get; set; }
        public int TurnNumber { get; set; }
    }
}
