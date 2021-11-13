namespace advisor.Persistence {
    using System.Collections.Generic;
    using HotChocolate;

    public record MapLevel(string Label, int Level, int Width, int Height);

    public class GameOptions {
        public List<MapLevel> Map { get; set; } = new ();
        public string Schedule { get; set; }
    }
}
