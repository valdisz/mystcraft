using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace advisor.Persistence;

public class DbOrders : InTurnContext {
    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public int TurnNumber { get; set; }
    public int UnitNumber { get; set; }

    public string Orders { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }
}

public class DbOrdersConfiguration : IEntityTypeConfiguration<DbOrders> {
    public DbOrdersConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbOrders> builder) {
        builder.HasKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });
    }
}
