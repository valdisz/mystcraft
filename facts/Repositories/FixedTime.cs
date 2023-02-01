namespace advisor.facts;

public class FixedTime : ITime {
    public FixedTime(DateTimeOffset time) {
        UtcNow = time;
    }

    public DateTimeOffset UtcNow { get; }
}
