using Microsoft.EntityFrameworkCore.Migrations;

namespace atlantis.Migrations
{
    public partial class next : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Population",
                table: "Regions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Race",
                table: "Regions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Population",
                table: "Regions");

            migrationBuilder.DropColumn(
                name: "Race",
                table: "Regions");
        }
    }
}
