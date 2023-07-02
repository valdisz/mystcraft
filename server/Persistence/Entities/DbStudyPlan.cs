namespace advisor.Persistence;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbStudyPlan : InTurnContext {
    public int UnitNumber { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    public DbSkill Target { get; set; }

    [MaxLength(Size.SKILL_CODE)]
    public string Study { get; set; }

    public List<int> Teach { get; set; } = new ();

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }

    [GraphQLIgnore]
    public DbUnit Unit { get; set; }
}

public class DbStudyPlanConfiguration : IEntityTypeConfiguration<DbStudyPlan> {
    public DbStudyPlanConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbStudyPlan> builder) {
        builder.HasKey(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });

        builder.Property(x => x.Teach)
            .HasConversionJson(db.Provider);

        builder.OwnsOne(p => p.Target, owned => {
            owned.Ignore(x => x.Days);
        });

        builder.HasOne(p => p.Unit)
            .WithOne(p => p.StudyPlan)
            .HasForeignKey<DbStudyPlan>(x => new { x.PlayerId, x.TurnNumber, x.UnitNumber });
    }
}
