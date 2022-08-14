namespace advisor.Persistence {
    using System.ComponentModel.DataAnnotations;
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [GraphQLName("FleetContent")]
    public class DbFleetContent {
        [MaxLength(64)]
        public string Type { get; set; }
        public int Count { get; set; }
    }
}
