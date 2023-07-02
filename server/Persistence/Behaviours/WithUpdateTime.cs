namespace advisor.Persistence;

using System;

/// <summary>
/// This interface is used to mark entities that have update time.
/// </summary>
public interface WithUpdateTime {
    DateTimeOffset UpdatedAt { get; set; }
}

