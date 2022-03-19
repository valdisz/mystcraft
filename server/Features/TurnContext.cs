namespace advisor.Features
{
    using advisor.Persistence;

    public record TurnContext : InTurnContext {
        public TurnContext(long gameId, long playerId, int turnNumber) {
            TurnNumber = turnNumber;
            GameId = gameId;
            PlayerId = playerId;
        }

        public TurnContext(TurnContext other) {
            GameId = other.GameId;
            PlayerId = other.PlayerId;
            TurnNumber = other.TurnNumber;
        }

        public long GameId { get; set; }
        public long PlayerId { get; set; }
        public int TurnNumber { get; set; }
    }
}
