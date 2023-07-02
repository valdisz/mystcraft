namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    public class DbFleetContent {
        [MaxLength(Size.TYPE)]
        public string Type { get; set; }

        public int Count { get; set; }
    }
}
