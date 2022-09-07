namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using HotChocolate;

public class DbGameReport : InGameContext {
    [GraphQLIgnore]
    public long GameId { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public int FactionNumber { get; set; }

    [Required]
    public byte[] Data { get; set; }

    public DbGame Game { get; set; }
    public DbGameTurn Turn { get; set; }
}
