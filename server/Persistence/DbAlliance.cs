namespace advisor.Persistence;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbAlliance : InGameContext {
    [GraphQLIgnore]
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public long GameId { get; set; }

    [Required]
    [MaxLength(Size.NAME)]
    public string Name { get; set; }

    public List<DbAllianceMember> Members { get; set; } = new ();

    [GraphQLIgnore]
    public DbGame Game { get; set; }
}

public class DbAllianceConfiguration : IEntityTypeConfiguration<DbAlliance> {
    public DbAllianceConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbAlliance> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.HasMany(p => p.Members)
            .WithOne(x => x.Alliance)
            .HasForeignKey(x => x.AllianceId);
    }
}
