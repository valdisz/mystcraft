namespace advisor;

using System;

public interface ITime {
    DateTimeOffset UtcNow { get; }
}
