namespace advisor.Persistence;

using System;

/// <summary>
/// This interface is used to mark entities that have creation time.
/// </summary>
public interface WithCreationTime {
    DateTimeOffset CreatedAt { get; set; }
}
