using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace advisor.Migrations.pgsql
{
    public partial class is_mage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMage",
                table: "Units",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMage",
                table: "Units");
        }
    }
}
