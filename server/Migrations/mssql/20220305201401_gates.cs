using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class gates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Gate",
                table: "Regions",
                type: "int",
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
