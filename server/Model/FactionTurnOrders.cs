namespace advisor.Model;

public record struct FactionOrders(FactionNumber Number, string Password, Seq<UnitOrders> Units) {
    public static FactionOrders New(FactionNumber number, string password, Seq<UnitOrders> units) => new (number, password, units);
}
