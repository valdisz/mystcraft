namespace advisor.Persistence;

public class DbOrders : InTurnContext {
    public long PlayerId { get; set; }
    public int TurnNumber { get; set; }
    public int UnitNumber { get; set; }

    public string Orders { get; set; }

    public DbPlayerTurn Turn { get; set; }
}
