namespace advisor.Persistence;

using System.ComponentModel.DataAnnotations;
using HotChocolate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DbArticle : InGameContext {
    [Key]
    public long Id { get; set; }

    [GraphQLIgnore]
    public long GameId { get; set; }

    [GraphQLIgnore]
    public int TurnNumber { get; set; }

    public long? PlayerId { get; set; }

    [Required, MaxLength(Size.TYPE)]
    public string Type { get; set; }

    [Required]
    public string Text { get; set; }

    [GraphQLIgnore]
    public DbGame Game { get; set; }

    [GraphQLIgnore]
    public DbTurn Turn { get; set; }
}

public class DbArticleConfiguration : IEntityTypeConfiguration<DbArticle> {
    public DbArticleConfiguration(Database db) {
        this.db = db;
    }

    private readonly Database db;

    public void Configure(EntityTypeBuilder<DbArticle> builder) {
        builder.Property(x => x.Id)
            .UseIdentityColumn();
    }
}
