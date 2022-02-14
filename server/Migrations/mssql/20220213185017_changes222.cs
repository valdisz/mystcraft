using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class changes222 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StrcutureNumber",
                table: "Units",
                newName: "StructureNumber");

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Z",
                table: "Units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "X",
                table: "Structures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Y",
                table: "Structures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Z",
                table: "Structures",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "X",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Z",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "X",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "Structures");

            migrationBuilder.DropColumn(
                name: "Z",
                table: "Structures");

            migrationBuilder.RenameColumn(
                name: "StructureNumber",
                table: "Units",
                newName: "StrcutureNumber");
        }
    }
}
