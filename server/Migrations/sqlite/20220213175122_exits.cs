using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.sqlite
{
    public partial class exits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Exits",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Exits",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Settlement_Name",
                table: "Exits",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settlement_Size",
                table: "Exits",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Terrain",
                table: "Exits",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "Exits",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "Exits",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Z",
                table: "Exits",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Exits");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Exits");

            migrationBuilder.DropColumn(
                name: "Settlement_Name",
                table: "Exits");

            migrationBuilder.DropColumn(
                name: "Settlement_Size",
                table: "Exits");

            migrationBuilder.DropColumn(
                name: "Terrain",
                table: "Exits");

            migrationBuilder.DropColumn(
                name: "X",
                table: "Exits");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "Exits");

            migrationBuilder.DropColumn(
                name: "Z",
                table: "Exits");
        }
    }
}
