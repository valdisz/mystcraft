using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.sqlite
{
    public partial class game_no_ruleset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ruleset",
                table: "Games");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "Ruleset",
                table: "Games",
                type: "BLOB",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
