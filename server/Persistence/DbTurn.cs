namespace advisor.Persistence;

using System.Collections.Generic;
using System.Text.RegularExpressions;
using HotChocolate;

public record TurnId(long GameId, int TurnNumber) {
    private static readonly Regex PATTERN = new Regex(@"^(\d+)\@(\d+)$", RegexOptions.Compiled);

    public static TurnId CreateFrom(DbTurn turn) => new TurnId(turn.GameId, turn.Number);

    public static TurnId CreateFrom(string s) {
        var m = PATTERN.Match(s);
        if (!m.Success) {
            return null;
        }

        return new TurnId(
            GameId: long.Parse(m.Groups[2].Value),
            TurnNumber: int.Parse(m.Groups[1].Value)
        );
    }

    public override string ToString() =>  $"{TurnNumber}@{GameId}";
}

public class DbTurn : InGameContext {
    [GraphQLIgnore]
    public string PublicId => TurnId.CreateFrom(this).ToString();

    [GraphQLIgnore]
    public long GameId { get; set; }

    public int Number { get; set; }

    public TurnState State { get; set; }

    [GraphQLIgnore]
    public byte[] PlayerData { get; set; }

    [GraphQLIgnore]
    public byte[] GameData { get; set; }

    [GraphQLIgnore]
    public DbGame Game { get; set; }

    [GraphQLIgnore]
    public List<DbArticle> Articles { get; set; } = new ();

    [GraphQLIgnore]
    public List<DbReport> Reports { get; set; } = new ();
}
