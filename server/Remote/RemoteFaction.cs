namespace advisor.Remote;

public record RemoteFaction(int? Number, string Name, bool OrdersSubmitted, bool TimesSubmitted) {
    public bool IsNew => Number == null;
}
