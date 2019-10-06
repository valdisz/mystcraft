namespace atlantis
{
    using Pidgin;
    using static Pidgin.Parser;
    using static Tokens;

    public class Population {
        public Population(string race, int count) {
            Race = race;
            Count = count;
        }

        public string Race { get; }
        public int Count { get; }
    }
}
