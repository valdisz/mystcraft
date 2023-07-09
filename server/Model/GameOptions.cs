namespace advisor.Model;

using System;
using System.Collections.Generic;

public record GameOptions {
    public List<MapLevel> Map { get; set; } = new ();
    public string Schedule { get; set; }
    public string TimeZone { get; set; }
    public string ServerAddress { get; set; }

    public DateTimeOffset? StartAt { get; set; }
    public DateTimeOffset? FinishAt { get; set; }
}
