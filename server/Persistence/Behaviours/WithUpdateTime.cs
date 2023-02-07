namespace advisor.Persistence;

using System;

public interface WithUpdateTime {
    DateTimeOffset UpdatedAt { get; set; }
}

