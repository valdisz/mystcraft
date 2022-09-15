namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using HotChocolate;

public class DbReport : InGameContext {
    [GraphQLIgnore]
    public long GameId { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public int FactionNumber { get; set; }

    [Required]
    public byte[] Data { get; set; }

    public byte[] Json { get; set; }

    public string Error { get; set; }

    public DbGame Game { get; set; }
    public DbTurn Turn { get; set; }
}
