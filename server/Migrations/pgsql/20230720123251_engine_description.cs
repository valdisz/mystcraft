using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations.pgsql
{
    public partial class engine_description : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "GameEngines",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "GameEngines");
        }
    }
}
