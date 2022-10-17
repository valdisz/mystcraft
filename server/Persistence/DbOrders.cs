using HotChocolate;

namespace advisor.Persistence;

public class DbOrders : InTurnContext {
    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public int TurnNumber { get; set; }
    public int UnitNumber { get; set; }

    public string Orders { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }
}
