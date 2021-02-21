namespace atlantis
{
    public class Item {
        public Item(string code, string name, int count) {
            Code = code;
            Name = name;
            Count = count;
        }

        public string Code { get; }
        public string Name { get; }
        public int Count { get; }
    }
}
