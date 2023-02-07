namespace advisor;

using System;

public class DefaultTime : ITime {
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
