namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public enum ReportType {
    Report,
    Map
}

public class DbAdditionalReport : InTurnContext {
    [GraphQLIgnore]
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public long PlayerId { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    [Required]
    [MaxLength(Size.NAME)]
    public string Name { get; set; }

    [Required]
    public ReportType Type { get; set; }

    [Required]
    public byte[] Source { get; set; }

    public byte[] Json { get; set; }

    public string Error { get; set; }


    [GraphQLIgnore]
    public DbPlayer Player { get; set; }

    [GraphQLIgnore]
    public DbPlayerTurn Turn { get; set; }
}

public class DbAdditionalReportConfiguration : IEntityTypeConfiguration<DbAdditionalReport> {
    public DbAdditionalReportConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbAdditionalReport> builder) {
    }
}
