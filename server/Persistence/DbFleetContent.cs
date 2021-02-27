namespace atlantis.Persistence
{
    using Microsoft.EntityFrameworkCore;

    [Owned]
    public class DbFleetContent {
        public string Type { get; set; }
        public int Count { get; set; }
    }
}
