namespace advisor.Model
{
    public class JEvent {
        public EventCategory Category { get; set; }
        public JUnitRef Unit { get; set; }
        public string Terrain { get; set; }
        public JCoords Coords { get; set; }
        public string Province { get; set; }
        public string Message { get; set; }
        public int? Amount { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? Price { get; set; }

        public override string ToString() => $"{Category}: {Message}";
    }
}
