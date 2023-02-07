namespace advisor.Persistence;

using System;

public interface WithCreationTime {
    DateTimeOffset CreatedAt { get; set; }
}
