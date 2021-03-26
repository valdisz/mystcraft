using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.pgsql
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
                type: "text",
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
