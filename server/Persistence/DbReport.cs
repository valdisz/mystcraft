namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using HotChocolate;

public class DbReport : InGameContext {
    [GraphQLIgnore]
    public long PlayerId { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public long GameId { get; set; }

    public int FactionNumber { get; set; }


    [Required]
    [GraphQLIgnore]
    public byte[] Data { get; set; }

    [GraphQLIgnore]
    public byte[] Json { get; set; }

    public string Error { get; set; }

    public bool IsParsed => (Json?.Length ?? 0) > 0;

    public bool IsMerged { get; set; }

    [GraphQLIgnore]
    public DbGame Game { get; set; }

    [GraphQLIgnore]
    public DbPlayer Player { get; set; }

    [GraphQLIgnore]
    public DbTurn Turn { get; set; }
}
