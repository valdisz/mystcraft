namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using advisor.Model;
using HotChocolate;

public enum StatisticsCategory {
    Produced,
    Bought,
    Sold,
    Consumed,
}

public class DbStatisticsItem : DbItem, InPlayerContext {
    public DbStatisticsItem() {

    }

    public DbStatisticsItem(AnItem other) {
        this.Code = other.Code;
        this.Amount = other.Amount;
    }


    [GraphQLIgnore]
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public StatisticsCategory Category { get; set; }
}

public class DbTurnStatisticsItem : DbStatisticsItem, InTurnContext {
    [GraphQLIgnore]
    public int TurnNumber {get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }
}

public class DbRegionStatisticsItem : DbStatisticsItem, InRegionContext {
    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public string RegionId { get; set; }
}
