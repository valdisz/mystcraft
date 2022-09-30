namespace advisor.Persistence;

public interface InTurnContext : InPlayerContext {
    int TurnNumber { get; set; }
}

public interface InRegionContext : InTurnContext {
    string RegionId { get; set; }
}
