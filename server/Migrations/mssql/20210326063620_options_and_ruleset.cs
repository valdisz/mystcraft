using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class options_and_ruleset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemoteGameOptions",
                table: "Games",
                newName: "Options");

            migrationBuilder.AddColumn<string>(
                name: "Ruleset",
                table: "Games",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ruleset",
                table: "Games");

            migrationBuilder.RenameColumn(
                name: "Options",
                table: "Games",
                newName: "RemoteGameOptions");
        }
    }
}
