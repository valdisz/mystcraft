namespace advisor.Persistence;

public interface InRegionContext : InTurnContext {
    string RegionId { get; set; }
}
