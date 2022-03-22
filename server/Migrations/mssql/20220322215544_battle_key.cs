using Microsoft.EntityFrameworkCore.Migrations;

namespace advisor.Migrations.mssql
{
    public partial class battle_key : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Battles",
                table: "Battles");

            migrationBuilder.RenameColumn(
                name: "GameId",
                table: "Battles",
                newName: "Id");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Battles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Battles",
                table: "Battles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_PlayerId_TurnNumber",
                table: "Battles",
                columns: new[] { "PlayerId", "TurnNumber" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Battles",
                table: "Battles");

            migrationBuilder.DropIndex(
                name: "IX_Battles_PlayerId_TurnNumber",
                table: "Battles");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Battles",
                newName: "GameId");

            migrationBuilder.AlterColumn<long>(
                name: "GameId",
                table: "Battles",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Battles",
                table: "Battles",
                columns: new[] { "PlayerId", "TurnNumber" });
        }
    }
}
