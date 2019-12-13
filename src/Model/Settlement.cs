namespace atlantis {
    public class Settlement {
        public Settlement(string name, SettlementSize size) {
            Name = name;
            Size = size;
        }

        public string Name { get; }
        public SettlementSize Size { get; }
    }
}
