namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using Microsoft.EntityFrameworkCore;

    [Owned]
    public class DbFleetContent {
        [MaxLength(64)]
        public string Type { get; set; }
        public int Count { get; set; }
    }
}
