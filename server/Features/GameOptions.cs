namespace advisor.Features {
    using System.Collections.Generic;

    public record MapLevel(string Label, int Level, int Width, int Height);

    public class GameOptions {
        public List<MapLevel> Map { get; set; } = new ();
    }
}
