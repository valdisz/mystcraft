namespace advisor.Persistence
{
    using HotChocolate;
    using Microsoft.EntityFrameworkCore;

    [GraphQLName("UserRole")]
    [Owned]
    public class DbUserRole {
        public string Role { get; set; }
    }
}
