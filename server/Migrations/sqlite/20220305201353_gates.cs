using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.sqlite
{
    public partial class gates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gate",
                table: "Regions",
                type: "INTEGER",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gate",
                table: "Regions");
        }
    }
}
