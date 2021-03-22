using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.sqlite
{
    public partial class clear_json_reports : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Reports SET Json = null");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
