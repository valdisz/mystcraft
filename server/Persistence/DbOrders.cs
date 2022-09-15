namespace advisor.Persistence;

public class DbOrders : InTurnContext {
    public int TurnNumber { get; set; }
    public long PlayerId { get; set; }
    public int UnitNumber { get; set; }
    public string Orders { get; set; }
}
