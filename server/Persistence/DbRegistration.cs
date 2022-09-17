namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using HotChocolate;

public class DbRegistration : InGameContext {
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public long GameId { get; set; }

    [GraphQLIgnore]
    public long UserId { get; set; }

    [MaxLength(128)]
    public string Name { get; set; }

    [MaxLength(64)]
    public string Password { get; set; }

    [GraphQLIgnore]
    public DbUser User { get;set; }

    [GraphQLIgnore]
    public DbGame Game { get;set; }
}
